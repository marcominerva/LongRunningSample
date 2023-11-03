using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using LongRunningSample.DataAccessLayer;
using LongRunningSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration.GetConnectionString("SqlConnection"));

builder.Services.AddHangfire(configuration =>
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"),
    new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
);

builder.Services.AddHangfireServer();

builder.Services.AddScoped<LongRunningService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/jobs");

var jobsGroupApi = app.MapGroup("/api/jobs");

jobsGroupApi.MapPost(string.Empty, async (LongRunningService longRunningService, LinkGenerator linkGenerator, HttpContext httpContext) =>
{
    var executionId = await longRunningService.ExecuteAsync();
    var url = linkGenerator.GetUriByName(httpContext, "GetStatus", new { id = executionId });

    return TypedResults.Accepted(url);
})
.WithOpenApi(operation =>
{
    operation.Summary = "Execute a long running job";
    return operation;
});

jobsGroupApi.MapGet("{id:guid}/status", async (LongRunningService longRunningService, Guid id) =>
{
    var status = await longRunningService.GetStatusAsync(id);
    return TypedResults.Ok(status);
})
.WithName("GetStatus")
.WithOpenApi();

jobsGroupApi.MapDelete("{id:guid}", async (LongRunningService longRunningService, Guid id) =>
{
    await longRunningService.CancelAsync(id);
    return TypedResults.NoContent();
})
.WithOpenApi();

app.Run();