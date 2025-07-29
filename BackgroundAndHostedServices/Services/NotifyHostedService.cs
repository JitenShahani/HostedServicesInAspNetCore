namespace BackgroundAndHostedServices.Services;

public class NotifyHostedService : IHostedService
{
	private readonly ILogger<NotifyHostedService> _logger;

	public NotifyHostedService (ILogger<NotifyHostedService> logger) =>
		_logger = logger;

	public async Task StartAsync (CancellationToken cancellationToken)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				for (var i = 1; i <= 5; i++)
				{
					_logger.LogInformation ("Hosted Service: Notifying user {number}", i);
					await Task.Delay (5000, cancellationToken);
				}
			}
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation ("Task was canceled gracefully...");
		}
	}

	public Task StopAsync (CancellationToken cancellationToken)
	{
		_logger.LogInformation ("Hosted Service: stopping service...");
		return Task.CompletedTask;
	}
}