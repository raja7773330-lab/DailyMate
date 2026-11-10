
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;

namespace RoutinelyApi.Controllers;

[ApiController]
[Route("api/system-health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase(
        CancellationToken cancellationToken)
    {
        try
        {
            bool connected = await _context.Database.CanConnectAsync(
                cancellationToken
            );

            if (!connected)
            {
                return StatusCode(503, new
                {
                    success = false,
                    status = "Disconnected",
                    database = "RoutinelyDB",
                    message = "Could not connect to SQL Server."
                });
            }

            return Ok(new
            {
                success = true,
                status = "Connected",
                database = "RoutinelyDB",
                message = "Database connection successful."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                status = "Error",
                message = ex.Message
            });
        }
    }
}
