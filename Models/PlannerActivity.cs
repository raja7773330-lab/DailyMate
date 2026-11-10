using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class PlannerActivity
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly ActivityDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Category { get; set; }

    public bool IsCompleted { get; set; }
}
