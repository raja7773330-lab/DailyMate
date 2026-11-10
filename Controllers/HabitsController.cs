
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using RoutinelyApi.Models;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HabitsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Habits
    [HttpGet]
    public async Task<IActionResult> GetHabits()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var habits = await _context.Habits
            .AsNoTracking()
            .Include(h => h.HabitLogs)
            .OrderBy(h => h.Title)
            .ToListAsync();

        var result = habits.Select(h => new
        {
            h.Id,
            h.Title,
            h.Description,
            h.Category,
            h.Frequency,
            WeeklyTarget = h.WeeklyTarget,
            ReminderTime = h.ReminderTime?.ToString("HH:mm"),
            h.ReminderEnabled,
            h.IsActive,

            IsCompletedToday = h.HabitLogs.Any(
                log => log.CompletedDate == today && log.IsCompleted
            ),

            CurrentStreak = CalculateCurrentStreak(h.HabitLogs),
            BestStreak = CalculateBestStreak(h.HabitLogs)
        });

        return Ok(result);
    }

    // GET: api/Habits/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetHabit(int id)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var habit = await _context.Habits
            .AsNoTracking()
            .Include(h => h.HabitLogs)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit == null)
            return NotFound(new { message = "Habit not found." });

        return Ok(new
        {
            habit.Id,
            habit.Title,
            habit.Description,
            habit.Category,
            habit.Frequency,
            WeeklyTarget = habit.WeeklyTarget,
            ReminderTime = habit.ReminderTime?.ToString("HH:mm"),
            habit.ReminderEnabled,
            habit.IsActive,

            IsCompletedToday = habit.HabitLogs.Any(
                log => log.CompletedDate == today && log.IsCompleted
            ),

            CurrentStreak = CalculateCurrentStreak(habit.HabitLogs),
            BestStreak = CalculateBestStreak(habit.HabitLogs)
        });
    }

    // POST: api/Habits
    [HttpPost]
    public async Task<IActionResult> CreateHabit(
        CreateHabitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Habit title is required." });

        if (request.WeeklyTarget < 1 || request.WeeklyTarget > 7)
            return BadRequest(new
            {
                message = "Weekly target must be between 1 and 7."
            });

        var habit = new Habit
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Category = request.Category,
            Frequency = request.Frequency,
            WeeklyTarget = request.WeeklyTarget,
            ReminderTime = request.ReminderTime,
            ReminderEnabled = request.ReminderEnabled,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetHabit),
            new { id = habit.Id },
            new
            {
                id = habit.Id,
                message = "Habit created successfully."
            }
        );
    }

    // PUT: api/Habits/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateHabit(
        int id,
        UpdateHabitRequest request)
    {
        var habit = await _context.Habits.FindAsync(id);

        if (habit == null)
            return NotFound(new { message = "Habit not found." });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Habit title is required." });

        if (request.WeeklyTarget < 1 || request.WeeklyTarget > 7)
            return BadRequest(new
            {
                message = "Weekly target must be between 1 and 7."
            });

        habit.Title = request.Title.Trim();
        habit.Description = request.Description;
        habit.Category = request.Category;
        habit.Frequency = request.Frequency;
        habit.WeeklyTarget = request.WeeklyTarget;
        habit.ReminderTime = request.ReminderTime;
        habit.ReminderEnabled = request.ReminderEnabled;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Habit updated successfully." });
    }

    // PATCH: api/Habits/5/complete
    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> ToggleHabitCompletion(int id)
    {
        var habit = await _context.Habits
            .Include(h => h.HabitLogs)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit == null)
            return NotFound(new { message = "Habit not found." });

        var today = DateOnly.FromDateTime(DateTime.Today);

        var todayLog = await _context.HabitLogs
            .FirstOrDefaultAsync(log =>
                log.HabitId == id &&
                log.CompletedDate == today);

        if (todayLog == null)
        {
            _context.HabitLogs.Add(new HabitLog
            {
                HabitId = id,
                CompletedDate = today,
                IsCompleted = true
            });
        }
        else
        {
            todayLog.IsCompleted = !todayLog.IsCompleted;
        }

        await _context.SaveChangesAsync();

        var logs = await _context.HabitLogs
            .AsNoTracking()
            .Where(log => log.HabitId == id)
            .ToListAsync();

        return Ok(new
        {
            message = "Habit completion updated.",

            isCompletedToday = logs.Any(
                log => log.CompletedDate == today && log.IsCompleted
            ),

            currentStreak = CalculateCurrentStreak(logs),
            bestStreak = CalculateBestStreak(logs)
        });
    }

    // DELETE: api/Habits/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHabit(int id)
    {
        var habit = await _context.Habits
            .Include(h => h.HabitLogs)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit == null)
            return NotFound(new { message = "Habit not found." });

        _context.HabitLogs.RemoveRange(habit.HabitLogs);
        _context.Habits.Remove(habit);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Habit deleted successfully." });
    }

    // Calculate consecutive completed days ending today or yesterday.
    private static int CalculateCurrentStreak(
        IEnumerable<HabitLog> logs)
    {
        var completedDates = logs
            .Where(log => log.IsCompleted)
            .Select(log => log.CompletedDate)
            .Distinct()
            .ToHashSet();

        var date = DateOnly.FromDateTime(DateTime.Today);

        if (!completedDates.Contains(date))
            date = date.AddDays(-1);

        var streak = 0;

        while (completedDates.Contains(date))
        {
            streak++;
            date = date.AddDays(-1);
        }

        return streak;
    }

    // Calculate the longest consecutive completion streak.
    private static int CalculateBestStreak(
        IEnumerable<HabitLog> logs)
    {
        var dates = logs
            .Where(log => log.IsCompleted)
            .Select(log => log.CompletedDate)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        var best = 0;
        var current = 0;
        DateOnly? previous = null;

        foreach (var date in dates)
        {
            if (previous.HasValue &&
                date == previous.Value.AddDays(1))
            {
                current++;
            }
            else
            {
                current = 1;
            }

            best = Math.Max(best, current);
            previous = date;
        }

        return best;
    }
}

public class CreateHabitRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Frequency { get; set; } = "Daily";
    public int WeeklyTarget { get; set; } = 7;
    public TimeOnly? ReminderTime { get; set; }
    public bool ReminderEnabled { get; set; } = false;
}

public class UpdateHabitRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Frequency { get; set; } = "Daily";
    public int WeeklyTarget { get; set; } = 7;
    public TimeOnly? ReminderTime { get; set; }
    public bool ReminderEnabled { get; set; } = false;
}
