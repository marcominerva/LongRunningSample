using Hangfire;
using LongRunningSample.DataAccessLayer;
using LongRunningSample.DataAccessLayer.Entities;
using LongRunningSample.Models;
using LongRunningSample.Models.Enums;

namespace LongRunningSample.Services;

public class LongRunningService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IBackgroundJobClient backgroundJobClient;

    public LongRunningService(ApplicationDbContext dbContext, IBackgroundJobClient backgroundJobClient)
    {
        this.dbContext = dbContext;
        this.backgroundJobClient = backgroundJobClient;
    }

    public async Task<Guid> ExecuteAsync()
    {
        var executionId = Guid.NewGuid();

        var jobId = backgroundJobClient.Schedule<LongRunningService>(service => service.ExecuteJobAsync(executionId, CancellationToken.None),
            DateTimeOffset.UtcNow.AddSeconds(10));

        var execution = new Execution
        {
            Id = executionId,
            Status = Status.NotRunning,
            CreatedAt = DateTime.UtcNow,
            JobId = jobId
        };

        dbContext.Executions.Add(execution);
        await dbContext.SaveChangesAsync();

        return executionId;
    }

    public async Task<ServiceStatus> GetStatusAsync(Guid id)
    {
        var execution = await dbContext.Executions.FindAsync(id);
        if (execution is null)
        {
            return new ServiceStatus
            {
                Status = Status.NotRunning
            };
        }

        return new ServiceStatus
        {
            Status = execution.Status,
            Exception = execution.Exception
        };
    }

    public async Task CancelAsync(Guid id)
    {
        var execution = await dbContext.Executions.FindAsync(id);
        if (execution is not null)
        {
            backgroundJobClient.Delete(execution.JobId);
        }
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var execution = await dbContext.Executions.FindAsync(id);
        if (execution is null)
        {
            return;
        }

        execution.Status = Status.Running;
        await dbContext.SaveChangesAsync();

        // Execution is running.
        for (var i = 0; i < 10; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                execution.Status = Status.Canceled;
                await dbContext.SaveChangesAsync();

                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        execution.Status = Status.Completed;
        execution.CompletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }
}
