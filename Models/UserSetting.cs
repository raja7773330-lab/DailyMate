using System;
using System.Collections.Generic;

namespace RoutinelyApi.Models;

public partial class UserSetting
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public bool NotificationsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }
}
