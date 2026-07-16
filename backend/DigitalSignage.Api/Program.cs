using DigitalSignage.Api.Data;
using Microsoft.EntityFrameworkCore;
using DigitalSignage.Api.Seeders;
using DigitalSignage.Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ejecutar seed inicial de base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}

app.Run();