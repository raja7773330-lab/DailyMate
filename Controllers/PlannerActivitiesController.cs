
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using RoutinelyApi.Models;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlannerActivitiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlannerActivitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/PlannerActivities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlannerActivity>>> GetActivities(
        [FromQuery] DateOnly? date)
    {
        var query = _context.PlannerActivities
            .AsNoTracking()
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(a => a.ActivityDate == date.Value);

        var activities = await query
            .OrderBy(a => a.ActivityDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return Ok(activities);
    }

    // GET: api/PlannerActivities/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlannerActivity>> GetActivity(int id)
    {
        var activity = await _context.PlannerActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
            return NotFound(new { message = "Planner activity not found." });

        return Ok(activity);
    }

    // POST: api/PlannerActivities
    [HttpPost]
    public async Task<ActionResult<PlannerActivity>> CreateActivity(
        PlannerActivityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Activity title is required." });

        if (request.StartTime.HasValue &&
            request.EndTime.HasValue &&
            request.EndTime.Value <= request.StartTime.Value)
        {
            return BadRequest(new
            {
                message = "End time must be later than start time."
            });
        }

        var activity = new PlannerActivity
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            ActivityDate = request.ActivityDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Category = request.Category,
            IsCompleted = false
        };

        _context.PlannerActivities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetActivity),
            new { id = activity.Id },
            activity
        );
    }

    // PUT: api/PlannerActivities/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateActivity(
        int id,
        PlannerActivityRequest request)
    {
        var activity = await _context.PlannerActivities.FindAsync(id);

        if (activity == null)
            return NotFound(new { message = "Planner activity not found." });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Activity title is required." });

        if (request.StartTime.HasValue &&
            request.EndTime.HasValue &&
            request.EndTime.Value <= request.StartTime.Value)
        {
            return BadRequest(new
            {
                message = "End time must be later than start time."
            });
        }

        activity.Title = request.Title.Trim();
        activity.Description = request.Description;
        activity.ActivityDate = request.ActivityDate;
        activity.StartTime = request.StartTime;
        activity.EndTime = request.EndTime;
        activity.Category = request.Category;

        await _context.SaveChangesAsync();

        return Ok(activity);
    }

    // PATCH: api/PlannerActivities/5/complete
    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> ToggleCompletion(int id)
    {
        var activity = await _context.PlannerActivities.FindAsync(id);

        if (activity == null)
            return NotFound(new { message = "Planner activity not found." });

        activity.IsCompleted = !activity.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Activity completion status updated.",
            activity.Id,
            activity.IsCompleted
        });
    }

    // DELETE: api/PlannerActivities/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var activity = await _context.PlannerActivities.FindAsync(id);

        if (activity == null)
            return NotFound(new { message = "Planner activity not found." });

        _context.PlannerActivities.Remove(activity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Planner activity deleted successfully." });
    }
}

public class PlannerActivityRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Category { get; set; }
}
