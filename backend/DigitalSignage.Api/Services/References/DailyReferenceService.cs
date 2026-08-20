using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.References;
using DigitalSignage.Api.Entities;
using DigitalSignage.Api.Services.References.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DigitalSignage.Api.Services.References;

public sealed class DailyReferenceService : IDailyReferenceService
{
    private const int MaxReferenceNumberLength = 50;
    private const int MaxRefreshAttempts = 2;
    private static readonly TimeSpan RefreshRetryDelay = TimeSpan.FromSeconds(2);

    private readonly ApplicationDbContext _dbContext;
    private readonly ISagaReferenceClient _sagaClient;
    private readonly ILogger<DailyReferenceService> _logger;

    public DailyReferenceService(
        ApplicationDbContext dbContext,
        ISagaReferenceClient sagaClient,
        ILogger<DailyReferenceService> logger)
    {
        _dbContext = dbContext;
        _sagaClient = sagaClient;
        _logger = logger;
    }

    public async Task<ReferenceLookupResponseDto?> LookupAsync(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        string normalized = Normalize(referenceNumber);

        _logger.LogInformation(
            "Consultando la referencia {ReferenceNumber} en SagaWS.",
            normalized);

        ExternalReferenceData? external = await _sagaClient.FindByReferenceAsync(
            normalized,
            cancellationToken);

        return external is null ? null : ToLookupDto(external);
    }

    public async Task<IReadOnlyList<DailyReferenceDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.DailyReferences
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DailyReferenceDto> CreateAsync(
        string referenceNumber,
        Guid createdByUserId,
        CancellationToken cancellationToken)
    {
        string normalized = Normalize(referenceNumber);

        if (await _dbContext.DailyReferences.AnyAsync(
                x => x.ReferenceNumber == normalized,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "La referencia ya fue agregada.");
        }

        ExternalReferenceData? external = await _sagaClient.FindByReferenceAsync(
            normalized,
            cancellationToken);

        if (external is null)
        {
            throw new KeyNotFoundException(
                "La referencia no fue encontrada en SagaWS.");
        }

        DateTime now = DateTime.UtcNow;
        var entity = new DailyReference
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            LastExternalUpdateAt = now
        };

        ApplyExternalData(entity, external);
        _dbContext.DailyReferences.Add(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _dbContext.Entry(entity).State = EntityState.Detached;
            throw new InvalidOperationException(
                "La referencia ya fue agregada.",
                exception);
        }

        _logger.LogInformation(
            "Referencia {ReferenceNumber} agregada con Id {ReferenceId}.",
            entity.ReferenceNumber,
            entity.Id);

        return ToDto(entity);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        DailyReference? entity = await _dbContext.DailyReferences
            .FindAsync([id], cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException("La referencia no existe.");
        }

        _dbContext.DailyReferences.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Referencia {ReferenceNumber} eliminada físicamente.",
            entity.ReferenceNumber);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken)
    {
        int deleted = await _dbContext.DailyReferences
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation(
            "Eliminación física total de referencias completada. Filas eliminadas: {Count}.",
            deleted);
    }

    public async Task<RefreshDailyReferencesResultDto> RefreshAllAsync(
        CancellationToken cancellationToken)
    {
        var currentReferences = await _dbContext.DailyReferences
            .AsNoTracking()
            .Select(x => new { x.Id, x.ReferenceNumber })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Actualización de referencias iniciada. Filas actuales: {Count}.",
            currentReferences.Count);

        int updated = 0;
        int notFound = 0;
        int failed = 0;

        foreach (var current in currentReferences)
        {
            try
            {
                ExternalReferenceData? external = await FindWithRetryAsync(
                    current.ReferenceNumber,
                    cancellationToken);

                if (external is null)
                {
                    _logger.LogWarning(
                        "SagaWS no devolvió temporalmente la referencia {ReferenceNumber}.",
                        current.ReferenceNumber);
                    notFound++;
                    continue;
                }

                DailyReference? entity = await _dbContext.DailyReferences
                    .FindAsync([current.Id], cancellationToken);

                if (entity is null)
                {
                    _logger.LogWarning(
                        "La referencia {ReferenceNumber} fue eliminada durante la actualización.",
                        current.ReferenceNumber);
                    continue;
                }

                ApplyExternalData(entity, external);
                entity.LastExternalUpdateAt = DateTime.UtcNow;

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    updated++;
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Conflicto de concurrencia al actualizar {ReferenceNumber}; probablemente fue eliminada.",
                        current.ReferenceNumber);
                    _dbContext.Entry(entity).State = EntityState.Detached;
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failed++;
                _logger.LogError(
                    exception,
                    "Falló la actualización individual de {ReferenceNumber}.",
                    current.ReferenceNumber);
                _dbContext.ChangeTracker.Clear();
            }
        }

        _logger.LogInformation(
            "Actualización de referencias terminada. Total: {TotalCount}, actualizadas: {RefreshedCount}, no encontradas: {NotFoundCount}, fallidas: {FailedCount}.",
            currentReferences.Count,
            updated,
            notFound,
            failed);

        return new RefreshDailyReferencesResultDto(
            currentReferences.Count,
            updated,
            notFound,
            failed,
            DateTime.UtcNow);
    }

    private async Task<ExternalReferenceData?> FindWithRetryAsync(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                ExternalReferenceData? result =
                    await _sagaClient.FindByReferenceAsync(
                    referenceNumber,
                    cancellationToken);

                if (result is not null || attempt >= MaxRefreshAttempts)
                {
                    return result;
                }

                _logger.LogWarning(
                    "SagaWS no devolvió {ReferenceNumber} en el intento {Attempt} de {MaxAttempts}; se reintentará.",
                    referenceNumber,
                    attempt,
                    MaxRefreshAttempts);

                await Task.Delay(RefreshRetryDelay, cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
                when (attempt < MaxRefreshAttempts && IsTransient(exception))
            {
                _logger.LogWarning(
                    exception,
                    "Falló el intento {Attempt} de {MaxAttempts} para {ReferenceNumber}; se reintentará.",
                    attempt,
                    MaxRefreshAttempts,
                    referenceNumber);

                await Task.Delay(RefreshRetryDelay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception exception) =>
        exception is HttpRequestException or OperationCanceledException;

    private static string Normalize(string referenceNumber)
    {
        string normalized = (referenceNumber ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("La referencia es obligatoria.");
        }

        if (normalized.Length > MaxReferenceNumberLength)
        {
            throw new ArgumentException(
                $"La referencia no puede exceder {MaxReferenceNumberLength} caracteres.");
        }

        return normalized;
    }

    private static void ApplyExternalData(
        DailyReference entity,
        ExternalReferenceData external)
    {
        entity.ReferenceNumber = external.ReferenceNumber;
        entity.ReferenceDate = external.ReferenceDate;
        entity.Client = external.Client;
        entity.OperationCode = external.OperationCode;
        entity.OperationDisplayName = external.OperationDisplayName;
        entity.Document = external.Document;
        entity.CustomsOfficeNumber = external.CustomsOfficeNumber;
        entity.CustomsOffice = external.CustomsOffice;
        entity.StatusCode = external.StatusCode;
        entity.StatusDescription = external.StatusDescription;
    }

    private static ReferenceLookupResponseDto ToLookupDto(
        ExternalReferenceData value) =>
        new(
            value.ReferenceNumber,
            value.ReferenceDate,
            value.Client,
            value.OperationCode,
            value.OperationDisplayName,
            value.Document,
            value.CustomsOfficeNumber,
            value.CustomsOffice,
            value.StatusCode,
            value.StatusDescription);

    private static DailyReferenceDto ToDto(DailyReference value) =>
        new(
            value.Id,
            value.ReferenceNumber,
            value.ReferenceDate,
            value.Client,
            value.OperationCode,
            value.OperationDisplayName,
            value.Document,
            value.CustomsOfficeNumber,
            value.CustomsOffice,
            value.StatusCode,
            value.StatusDescription,
            value.LastExternalUpdateAt,
            value.CreatedAt);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
}
