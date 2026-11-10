
using System;

namespace RoutinelyApi.Models;

public partial class Routine
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Category { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    // Reminder settings
    public string Repeat { get; set; } = "Every day";

    public bool ReminderEnabled { get; set; } = false;
}
