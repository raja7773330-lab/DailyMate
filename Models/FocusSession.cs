using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class FocusSession
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int DurationMinutes { get; set; }

    public DateTime CreatedAt { get; set; }
}
