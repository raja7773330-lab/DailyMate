
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using RoutinelyApi.Models;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static readonly string[] AllowedThemes =
    {
        "Pink",
        "Blue",
        "Green",
        "Purple"
    };

    // GET: api/UserSettings
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _context.UserSettings
            .OrderBy(s => s.Id)
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            return NotFound(new
            {
                message = "User settings not found."
            });
        }

        return Ok(settings);
    }

    // GET: api/UserSettings/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSetting(int id)
    {
        var setting = await _context.UserSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound(new
            {
                message = "User setting not found."
            });
        }

        return Ok(setting);
    }

    // POST: api/UserSettings
    [HttpPost]
    public async Task<IActionResult> CreateSettings(
        CreateUserSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest(new
            {
                message = "User name is required."
            });
        }

        if (!AllowedThemes.Contains(
            request.Theme,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Theme must be Pink, Blue, Green, or Purple."
            });
        }

        var existingSettings = await _context.UserSettings
            .FirstOrDefaultAsync();

        if (existingSettings != null)
        {
            return Conflict(new
            {
                message = "User settings already exist. Use PUT to update them."
            });
        }

        var setting = new UserSetting
        {
            UserName = request.UserName.Trim(),
            Theme = AllowedThemes.First(t =>
                t.Equals(request.Theme, StringComparison.OrdinalIgnoreCase)),
            NotificationsEnabled = request.NotificationsEnabled,
            CreatedAt = DateTime.Now
        };

        _context.UserSettings.Add(setting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSetting),
            new { id = setting.Id },
            setting
        );
    }

    // PUT: api/UserSettings/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSettings(
        int id,
        UpdateUserSettingRequest request)
    {
        var setting = await _context.UserSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound(new
            {
                message = "User setting not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest(new
            {
                message = "User name is required."
            });
        }

        if (!AllowedThemes.Contains(
            request.Theme,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Theme must be Pink, Blue, Green, or Purple."
            });
        }

        setting.UserName = request.UserName.Trim();
        setting.Theme = AllowedThemes.First(t =>
            t.Equals(request.Theme, StringComparison.OrdinalIgnoreCase));
        setting.NotificationsEnabled = request.NotificationsEnabled;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User settings updated successfully.",
            data = setting
        });
    }

    // PATCH: api/UserSettings/1/theme
    [HttpPatch("{id}/theme")]
    public async Task<IActionResult> UpdateTheme(
        int id,
        UpdateThemeRequest request)
    {
        var setting = await _context.UserSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound(new
            {
                message = "User setting not found."
            });
        }

        if (!AllowedThemes.Contains(
            request.Theme,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Theme must be Pink, Blue, Green, or Purple."
            });
        }

        setting.Theme = AllowedThemes.First(t =>
            t.Equals(request.Theme, StringComparison.OrdinalIgnoreCase));

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Theme updated successfully.",
            theme = setting.Theme
        });
    }

    // PATCH: api/UserSettings/1/notifications
    [HttpPatch("{id}/notifications")]
    public async Task<IActionResult> UpdateNotifications(
        int id,
        UpdateNotificationsRequest request)
    {
        var setting = await _context.UserSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound(new
            {
                message = "User setting not found."
            });
        }

        setting.NotificationsEnabled = request.NotificationsEnabled;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Notification preference updated successfully.",
            notificationsEnabled = setting.NotificationsEnabled
        });
    }
}

public class CreateUserSettingRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Theme { get; set; } = "Pink";
    public bool NotificationsEnabled { get; set; } = true;
}

public class UpdateUserSettingRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Theme { get; set; } = "Pink";
    public bool NotificationsEnabled { get; set; }
}

public class UpdateThemeRequest
{
    public string Theme { get; set; } = string.Empty;
}

public class UpdateNotificationsRequest
{
    public bool NotificationsEnabled { get; set; }
}
