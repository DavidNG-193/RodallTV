using DigitalSignage.Api.Data;
using Microsoft.EntityFrameworkCore;
using DigitalSignage.Api.Seeders;
using DigitalSignage.Api.Services;
using DigitalSignage.Api.Services.ExchangeRates;
using DigitalSignage.Api.Services.Weather;
using DigitalSignage.Api.Services.References;
using DigitalSignage.Api.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.Tools;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Evita que Windows Event Log convierta un fallo externo controlable en una
// conexión abortada cuando la cuenta del proceso no tiene permisos de escritura.
// Console funciona tanto en desarrollo como bajo systemd en Raspberry/Linux.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<MediaFolderService>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<PlaylistItemService>();
builder.Services.AddScoped<PlaylistAssignmentService>();
builder.Services.AddScoped<SyncLogService>();
builder.Services.AddScoped<DeviceAuthenticationService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<IDeviceExchangeRateSettingsService, DeviceExchangeRateSettingsService>();
builder.Services.AddScoped<IAgentExchangeRateService, AgentExchangeRateService>();
builder.Services.AddScoped<IDeviceWeatherSettingsService, DeviceWeatherSettingsService>();
builder.Services.AddScoped<IAgentReferenceService, AgentReferenceService>();
builder.Services.AddScoped<UserSessionValidationService>();
builder.Services.AddScoped<UsersService>();

// Authentication - JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
    var jwtAudience = builder.Configuration["Jwt:Audience"]!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var sessionValidator =
                context.HttpContext.RequestServices
                    .GetRequiredService<
                        UserSessionValidationService>();

            var isValid =
                await sessionValidator
                    .IsSessionValidAsync(
                        context.Principal!);

            if (!isValid)
            {
                context.Fail(
                    "La sesión ya no es válida."
                );
            }
        }
    };
});

// Conexión a PostgreSQL con Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Limite de carga de archivos
const long maxUploadSize = 500L * 1024L * 1024L;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadSize;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadSize;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.Configure<BanxicoOptions>(
    builder.Configuration.GetSection(BanxicoOptions.SectionName));

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<
    IExchangeRateProvider,
    BanxicoExchangeRateProvider>((serviceProvider, client) =>
{
    BanxicoOptions options = serviceProvider
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<BanxicoOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(
        options.RequestTimeoutSeconds);
});

builder.Services.AddOptions<WeatherOptions>()
    .Bind(builder.Configuration.GetSection(WeatherOptions.SectionName))
    .Validate(
        options => options.CacheMinutes > 0,
        "Weather:CacheMinutes debe ser mayor que cero.")
    .Validate(
        options => options.RequestTimeoutSeconds > 0,
        "Weather:RequestTimeoutSeconds debe ser mayor que cero.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.DefaultSetting.LocationName) &&
            options.DefaultSetting.Latitude is >= -90 and <= 90 &&
            options.DefaultSetting.Longitude is >= -180 and <= 180 &&
            !string.IsNullOrWhiteSpace(options.DefaultSetting.Timezone),
        "Weather:DefaultSetting contiene datos inválidos.")
    .ValidateOnStart();

builder.Services.AddScoped<
    IAgentWeatherService,
    AgentWeatherService>();

builder.Services.AddHttpClient<
    IWeatherProvider,
    OpenMeteoWeatherProvider>((serviceProvider, client) =>
{
    WeatherOptions options = serviceProvider
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<WeatherOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(
        options.RequestTimeoutSeconds);
});

builder.Services.AddOptions<SagaOptions>()
    .Bind(builder.Configuration.GetSection(SagaOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out Uri? baseUri) &&
            (baseUri.Scheme == Uri.UriSchemeHttp ||
             baseUri.Scheme == Uri.UriSchemeHttps),
        "Saga:BaseUrl debe ser una URL HTTP(S) absoluta.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ReferencePath),
        "Saga:ReferencePath es obligatorio.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.User) &&
            !string.IsNullOrWhiteSpace(options.Password),
        "Saga:User y Saga:Password son obligatorios.")
    .Validate(
        options => options.RequestTimeoutSeconds > 0,
        "Saga:RequestTimeoutSeconds debe ser mayor que cero.")
    .Validate(
        options => options.RefreshMinutes > 0,
        "Saga:RefreshMinutes debe ser mayor que cero.")
    .ValidateOnStart();

builder.Services.AddScoped<IDailyReferenceService, DailyReferenceService>();
builder.Services.AddHostedService<DailyReferenceRefreshBackgroundService>();

builder.Services.AddHttpClient<ISagaReferenceClient, SagaReferenceClient>(
    (serviceProvider, client) =>
    {
        SagaOptions options = serviceProvider
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<SagaOptions>>()
            .Value;

        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
});

builder.Services.AddAuthorization();

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    PermissionAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

app.UseCors("Frontend");
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (args.Contains("--reset-admin-password"))
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var email =
        Environment.GetEnvironmentVariable(
            "RODALL_RESET_ADMIN_EMAIL");

    var password =
        Environment.GetEnvironmentVariable(
            "RODALL_RESET_ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(email)
        || string.IsNullOrWhiteSpace(password))
    {
        Console.Error.WriteLine(
            "Faltan variables de recuperación.");
        return;
    }

    if (password.Length is < 8 or > 100)
    {
        Console.Error.WriteLine(
            "La contraseña temporal debe tener entre 8 y 100 caracteres.");
        return;
    }

    var success =
        await AdminPasswordResetTool.ResetAsync(
            db,
            email,
            password);

    Console.WriteLine(
        success
            ? "Contraseña administrativa restablecida."
            : "No se encontró el administrador.");

    return;
}

// Ejecutar seed inicial de base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var weatherOptions = scope.ServiceProvider
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<WeatherOptions>>()
        .Value;
    await DatabaseSeeder.SeedAsync(context, weatherOptions);
}

app.Run();
