using Hangfire;
using Hangfire.MemoryStorage;
using LogEvac.Extensions;
using Project_SerilogDI.helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.InitializeSerilog();

// Log Evac Pre-requisites
builder.Services.AddHangfire(config =>
{
    // Note: For production scenarios, consider using a more robust storage option like SQL Server, Redis, or PostgreSQL instead of in-memory storage.
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage();
});
builder.Services.AddHangfireServer();

// Implement Log Evac
builder.InitializeLogEvac();

var app = builder.Build();

// Log Evac Pre-requisites: Hangfire Dashboard
app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Run Log Evac Scheduler
// Depending on the appsettings.json [LogEvacSettings] configuration, this will schedule the Log Evac job to run at the specified intervals (e.g., every hour).
app.ScheduleLogEvacJob();

app.Run();
