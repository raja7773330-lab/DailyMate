
using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class Habit
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string Frequency { get; set; } = null!;

    public int WeeklyTarget { get; set; }

    public TimeOnly? ReminderTime { get; set; }

    // Habit notification setting
    public bool ReminderEnabled { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<HabitLog> HabitLogs { get; set; } = new List<HabitLog>();
}
