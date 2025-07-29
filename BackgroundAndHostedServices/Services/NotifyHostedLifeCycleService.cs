namespace BackgroundAndHostedServices.Services;

public class NotifyHostedLifeCycleService : IHostedLifecycleService
{
	private readonly ILogger<NotifyHostedLifeCycleService> _logger;

	public NotifyHostedLifeCycleService (ILogger<NotifyHostedLifeCycleService> logger) =>
		_logger = logger;

	public async Task StartingAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Starting Async...");
		await Task.Delay (5000, cancellationToken);
	}

	public async Task StartAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Start Async...");
		await Task.Delay (5000, cancellationToken);
	}

	public async Task StartedAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Started Async...");
		await Task.Delay (5000, cancellationToken);
	}

	public Task StoppingAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Stopping Async...");
		return Task.CompletedTask;
	}

	public Task StopAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Stop Async...");
		return Task.CompletedTask;
	}

	public Task StoppedAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Lifecycle Service: Stopped Async...");
		return Task.CompletedTask;
	}
}