
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;
using TaskModel = RoutinelyApi.Models.Task;

namespace RoutinelyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks()
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks);
    }

    // GET: api/Tasks/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskModel>> GetTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        return Ok(task);
    }

    // POST: api/Tasks
    [HttpPost]
    public async Task<ActionResult<TaskModel>> CreateTask(
        CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Task title is required." });

        if (!IsValidPriority(request.Priority))
            return BadRequest(new
            {
                message = "Priority must be Low, Medium or High."
            });

        var task = new TaskModel
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Priority = request.Priority,
            Category = request.Category,
            DueDate = request.DueDate,
            DueTime = request.DueTime,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTask),
            new { id = task.Id },
            task
        );
    }

    // PUT: api/Tasks/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(
        int id,
        UpdateTaskRequest request)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Task title is required." });

        if (!IsValidPriority(request.Priority))
            return BadRequest(new
            {
                message = "Priority must be Low, Medium or High."
            });

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.Category = request.Category;
        task.DueDate = request.DueDate;
        task.DueTime = request.DueTime;
        task.IsCompleted = request.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // PATCH: api/Tasks/5/complete
    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        task.IsCompleted = !task.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Task completion updated.",
            task.Id,
            task.IsCompleted
        });
    }

    // DELETE: api/Tasks/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound(new { message = "Task not found." });

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Task deleted successfully." });
    }

    private static bool IsValidPriority(string? priority)
    {
        return priority is not null &&
            (priority.Equals("Low", StringComparison.OrdinalIgnoreCase) ||
             priority.Equals("Medium", StringComparison.OrdinalIgnoreCase) ||
             priority.Equals("High", StringComparison.OrdinalIgnoreCase));
    }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? Category { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DueTime { get; set; }
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? Category { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DueTime { get; set; }
    public bool IsCompleted { get; set; }
}
