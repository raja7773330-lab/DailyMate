using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class HabitLog
{
    public int Id { get; set; }

    public int HabitId { get; set; }

    public DateOnly CompletedDate { get; set; }

    public bool IsCompleted { get; set; }

    public virtual Habit Habit { get; set; } = null!;
}
