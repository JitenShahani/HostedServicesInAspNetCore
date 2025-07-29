namespace BackgroundAndHostedServices.Services;

public class NotifyBackgroundService : BackgroundService
{
	private readonly ILogger<NotifyBackgroundService> _logger;

	public NotifyBackgroundService (ILogger<NotifyBackgroundService> logger) =>
		_logger = logger;

	protected override async Task ExecuteAsync (CancellationToken stoppingToken)
	{
		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				for (var i = 1; i <= 5; i++)
				{
					_logger.LogInformation ("Background Service: Notifying user {number}", i);
					await Task.Delay (5000, stoppingToken);
				}
			}
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation ("Task was canceled gracefully...");
		}
	}
}