
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Models;
using TaskModel = RoutinelyApi.Models.Task;

namespace RoutinelyApi.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FocusSession> FocusSessions { get; set; }

    public virtual DbSet<Habit> Habits { get; set; }

    public virtual DbSet<HabitLog> HabitLogs { get; set; }

    public virtual DbSet<PlannerActivity> PlannerActivities { get; set; }

    public virtual DbSet<Routine> Routines { get; set; }

    public virtual DbSet<TaskModel> Tasks { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FocusSession>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Category).HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description).HasMaxLength(500);

            entity.Property(e => e.Frequency)
                .HasMaxLength(30)
                .HasDefaultValue("Daily");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.Title).HasMaxLength(150);

            entity.Property(e => e.WeeklyTarget)
                .HasDefaultValue(7);

            entity.Property(e => e.ReminderEnabled)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<HabitLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(true);

            entity.HasOne(d => d.Habit)
                .WithMany(p => p.HabitLogs)
                .HasForeignKey(d => d.HabitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PlannerActivity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        modelBuilder.Entity<Routine>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Category).HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.Property(e => e.Repeat)
                .HasMaxLength(30)
                .HasDefaultValue("Every day");

            entity.Property(e => e.ReminderEnabled)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<TaskModel>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Category).HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description).HasMaxLength(500);

            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            entity.Property(e => e.Title).HasMaxLength(150);
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.NotificationsEnabled)
                .HasDefaultValue(true);

            entity.Property(e => e.Theme)
                .HasMaxLength(30)
                .HasDefaultValue("Pink");

            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasDefaultValue("Routinely User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
