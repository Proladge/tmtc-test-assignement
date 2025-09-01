namespace TaskManagementApi.Services;

public class TaskReassignmentService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskReassignmentService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(2); // Run every 2 minutes

    public TaskReassignmentService(IServiceProvider serviceProvider, ILogger<TaskReassignmentService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Reassignment Service started. Running every 2 minutes.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during task reassignment: {Message}", ex.Message);
            }
            
            await Task.Delay(_period, stoppingToken);
        }
    }

    private async Task DoWorkAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
        
        _logger.LogInformation("Starting task reassignment cycle at {Time}", DateTime.UtcNow);
        
        try
        {
            await dataService.ReassignTasksAsync();
            _logger.LogInformation("Task reassignment cycle completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during task reassignment: {Message}", ex.Message);
        }
    }
}
