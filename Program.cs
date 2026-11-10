
using Microsoft.EntityFrameworkCore;
using RoutinelyApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<RoutinelyApi.Services.GeminiService>();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterWeb", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost") ||
                origin.StartsWith("http://127.0.0.1"))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();
app.UseCors("FlutterWeb");
app.MapGet("/api/health/db", async (ApplicationDbContext db) =>
{
    try
    {
        bool connected = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            success = connected,
            message = connected
                ? "Connected to RoutinelyDB successfully!"
                : "Database connection failed."
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
