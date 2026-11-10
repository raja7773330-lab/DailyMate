using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class Task
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Priority { get; set; } = null!;

    public string? Category { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? DueTime { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
}