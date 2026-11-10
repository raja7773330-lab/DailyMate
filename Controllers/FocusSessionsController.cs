
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using RoutinelyApi.Models;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FocusSessionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FocusSessionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/FocusSessions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FocusSession>>> GetSessions()
    {
        var sessions = await _context.FocusSessions
            .AsNoTracking()
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return Ok(sessions);
    }

    // GET: api/FocusSessions/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FocusSession>> GetSession(int id)
    {
        var session = await _context.FocusSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Focus session not found." });

        return Ok(session);
    }

    // POST: api/FocusSessions/start
    [HttpPost("start")]
    public async Task<ActionResult<FocusSession>> StartSession()
    {
        bool runningSessionExists = await _context.FocusSessions
            .AnyAsync(s => s.EndTime == null);

        if (runningSessionExists)
            return Conflict(new
            {
                message = "A focus session is already running."
            });

        var session = new FocusSession
        {
            StartTime = DateTime.Now,
            EndTime = null,
            DurationMinutes = 0,
            CreatedAt = DateTime.Now
        };

        _context.FocusSessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSession),
            new { id = session.Id },
            session
        );
    }

    // PATCH: api/FocusSessions/5/stop

    [HttpPatch("{id:int}/stop")]
    public async Task<IActionResult> StopSession(
        int id,
        [FromBody] StopFocusSessionRequest? request)
    {
        var session = await _context.FocusSessions.FindAsync(id);

        if (session == null)
            return NotFound(new { message = "Focus session not found." });

        if (session.EndTime != null)
            return BadRequest(new
            {
                message = "This focus session has already stopped."
            });

        session.EndTime = DateTime.Now;

        if (request?.DurationSeconds != null)
        {
            if (request.DurationSeconds < 0)
                return BadRequest(new { message = "Duration cannot be negative." });

            session.DurationMinutes = request.DurationSeconds.Value / 60;
        }
        else
        {
            session.DurationMinutes = Math.Max(
                0,
                (int)(session.EndTime.Value - session.StartTime).TotalMinutes
            );
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Focus session completed successfully.",
            session.Id,
            session.StartTime,
            session.EndTime,
            session.DurationMinutes
        });
    }

    public class StopFocusSessionRequest
    {
        public int? DurationSeconds { get; set; }
    }


    // GET: api/FocusSessions/running
    [HttpGet("running")]
    public async Task<ActionResult<FocusSession>> GetRunningSession()
    {
        var session = await _context.FocusSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.EndTime == null);

        if (session == null)
            return NotFound(new { message = "No active focus session." });

        return Ok(session);
    }

    // GET: api/FocusSessions/stats/daily
    [HttpGet("stats/daily")]
    public async Task<IActionResult> GetDailyStats(
        [FromQuery] DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        var start = selectedDate.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var sessions = await _context.FocusSessions
            .AsNoTracking()
            .Where(s => s.StartTime >= start && s.StartTime < end)
            .ToListAsync();

        return Ok(new
        {
            date = selectedDate,
            totalSessions = sessions.Count,
            completedSessions = sessions.Count(s => s.EndTime != null),
            totalFocusMinutes = sessions.Sum(s => s.DurationMinutes)
        });
    }

    // DELETE: api/FocusSessions/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var session = await _context.FocusSessions.FindAsync(id);

        if (session == null)
            return NotFound(new { message = "Focus session not found." });

        _context.FocusSessions.Remove(session);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Focus session deleted successfully." });
    }
}
