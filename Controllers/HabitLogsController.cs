
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using RoutinelyApi.Models;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HabitLogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HabitLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/HabitLogs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HabitLog>>> GetHabitLogs(
        [FromQuery] int? habitId)
    {
        var query = _context.HabitLogs
            .AsNoTracking()
            .AsQueryable();

        if (habitId.HasValue)
        {
            query = query.Where(log => log.HabitId == habitId.Value);
        }

        var logs = await query
            .OrderByDescending(log => log.CompletedDate)
            .ToListAsync();

        return Ok(logs);
    }

    // GET: api/HabitLogs/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HabitLog>> GetHabitLog(int id)
    {
        var log = await _context.HabitLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(log => log.Id == id);

        if (log == null)
            return NotFound(new { message = "Habit log not found." });

        return Ok(log);
    }

    // GET: api/HabitLogs/habit/5
    [HttpGet("habit/{habitId:int}")]
    public async Task<ActionResult<IEnumerable<HabitLog>>> GetLogsByHabit(
        int habitId)
    {
        var habitExists = await _context.Habits
            .AnyAsync(h => h.Id == habitId);

        if (!habitExists)
            return NotFound(new { message = "Habit not found." });

        var logs = await _context.HabitLogs
            .AsNoTracking()
            .Where(log => log.HabitId == habitId)
            .OrderByDescending(log => log.CompletedDate)
            .ToListAsync();

        return Ok(logs);
    }

    // POST: api/HabitLogs
    [HttpPost]
    public async Task<ActionResult<HabitLog>> CreateHabitLog(
        CreateHabitLogRequest request)
    {
        var habit = await _context.Habits
            .FindAsync(request.HabitId);

        if (habit == null)
            return NotFound(new { message = "Habit not found." });

        var completedDate = request.CompletedDate ?? DateOnly.FromDateTime(DateTime.Today);

        var existingLog = await _context.HabitLogs
            .FirstOrDefaultAsync(log =>
                log.HabitId == request.HabitId &&
                log.CompletedDate == completedDate);

        if (existingLog != null)
            return Conflict(new
            {
                message = "A log already exists for this habit on this date."
            });

        var log = new HabitLog
        {
            HabitId = request.HabitId,
            CompletedDate = completedDate,
            IsCompleted = request.IsCompleted
        };

        _context.HabitLogs.Add(log);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetHabitLog),
            new { id = log.Id },
            log
        );
    }

    // PATCH: api/HabitLogs/5/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ToggleHabitLogStatus(int id)
    {
        var log = await _context.HabitLogs.FindAsync(id);

        if (log == null)
            return NotFound(new { message = "Habit log not found." });

        log.IsCompleted = !log.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Habit completion status updated.",
            log.Id,
            log.HabitId,
            log.CompletedDate,
            log.IsCompleted
        });
    }

    // DELETE: api/HabitLogs/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHabitLog(int id)
    {
        var log = await _context.HabitLogs.FindAsync(id);

        if (log == null)
            return NotFound(new { message = "Habit log not found." });

        _context.HabitLogs.Remove(log);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Habit log deleted successfully." });
    }
}

public class CreateHabitLogRequest
{
    public int HabitId { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public bool IsCompleted { get; set; } = true;
}
