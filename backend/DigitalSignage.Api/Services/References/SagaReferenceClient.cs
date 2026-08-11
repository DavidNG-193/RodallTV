using System.Net.Http.Json;
using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Services.References.Models;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services.References;

public sealed class SagaReferenceClient : ISagaReferenceClient
{
    private readonly HttpClient _httpClient;
    private readonly SagaOptions _options;
    private readonly ILogger<SagaReferenceClient> _logger;

    public SagaReferenceClient(
        HttpClient httpClient,
        IOptions<SagaOptions> options,
        ILogger<SagaReferenceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExternalReferenceData?> FindByReferenceAsync(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        string normalized = referenceNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("La referencia es obligatoria.");

        SagaReferenceRequest payload =
            SagaReferenceRequestFactory.ForReference(normalized);

        using var request =
            new HttpRequestMessage(HttpMethod.Post, _options.ReferencePath);

        request.Headers.Add("User", _options.User);
        request.Headers.Add("Password", _options.Password);
        request.Content = JsonContent.Create(payload);

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "SagaWS excedió el tiempo límite de respuesta.");
            throw;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "No fue posible conectar con SagaWS.");
            throw;
        }

        using (response)
        {
            string raw = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SagaWS respondió HTTP {StatusCode}.",
                    (int)response.StatusCode);

                throw new HttpRequestException(
                    $"SagaWS respondió HTTP {(int)response.StatusCode}.");
            }

            SagaReferenceResponse? result;

            try
            {
                result = System.Text.Json.JsonSerializer.Deserialize<
                    SagaReferenceResponse>(
                    raw,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (System.Text.Json.JsonException exception)
            {
                _logger.LogError(exception, "SagaWS devolvió JSON inválido.");
                throw new SagaReferenceResponseException(
                    "SagaWS devolvió una respuesta inválida.", exception);
            }

            if (result?.References is null || result.References.Count == 0)
                return null;

            SagaReferenceItem? exact = result.References.FirstOrDefault(x =>
                string.Equals(
                    x.Referencia?.Trim(),
                    normalized,
                    StringComparison.OrdinalIgnoreCase));

            return exact is null ? null : SagaReferenceMapper.Map(exact);
        }
    }
}
