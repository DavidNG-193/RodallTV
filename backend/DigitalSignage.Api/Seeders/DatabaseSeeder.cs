using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        WeatherOptions weatherOptions)
    {
        bool adminExists = await context.Users.AnyAsync(u => u.Email == "gherson@rodall.com");

        if (!adminExists)
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "System",
                Email = "gherson@rodall.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                Role = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        var missingWeatherDeviceIds = await context.Devices
            .Where(device => !context.DeviceWeatherSettings
                .Any(setting => setting.DeviceId == device.Id))
            .Select(device => device.Id)
            .ToListAsync();

        if (missingWeatherDeviceIds.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        WeatherDefaultSettingOptions defaults = weatherOptions.DefaultSetting;
        var settings = missingWeatherDeviceIds.Select(deviceId =>
            new DeviceWeatherSetting
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                LocationName = defaults.LocationName.Trim(),
                Latitude = defaults.Latitude,
                Longitude = defaults.Longitude,
                Timezone = defaults.Timezone.Trim(),
                IsActive = defaults.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            });

        context.DeviceWeatherSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }
}
