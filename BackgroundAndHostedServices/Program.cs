var builder = WebApplication.CreateBuilder (args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://@aka.ms/aspnet/openapi
builder.Services.AddOpenApi ();

builder.Services.Configure<JsonOptions> (options =>
{
	// Configure JSON serializer to ignore null values during serialization
	options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

	// Configure JSON serializer to use Pascal case for property names during serialization
	options.SerializerOptions.PropertyNamingPolicy = null;

	// Configure JSON serializer to use Pascal case for key's name during serialization
	options.SerializerOptions.DictionaryKeyPolicy = null;

	// Ensure JSON property names are not case-sensitive during deserialization
	options.SerializerOptions.PropertyNameCaseInsensitive = true;

	// Prevent serialization issues caused by cyclic relationships in EF Core entities
	options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

	// Ensure the JSON output is consistently formatted for readability.
	// Not to be used in Production as the response message size could be large
	// options.SerializerOptions.WriteIndented = true;
});

// builder.Services.Configure<HostOptions> (options =>
// {
// 	// Start all services in parallel to improve readiness.
// 	options.ServicesStartConcurrently = true;

// 	// Stop all services in parallel for quicker shutdown.
// 	options.ServicesStopConcurrently = true;
// });

_ = Random.Shared.Next (0, 3) switch
{
	0 => builder.Services.AddHostedService<NotifyBackgroundService> (),
	1 => builder.Services.AddHostedService<NotifyHostedService> (),
	2 => builder.Services.AddHostedService<NotifyHostedLifeCycleService> (),
	_ => throw new InvalidOperationException ("Unexpected value for random")
};

var app = builder.Build ();

/*
app.Services.GetRequiredService<T>() returns a registered services of type T. But, the sequence of the registered services is not guaranteed.
Hence, I am using app.Services.GetService<T>() to get the first registered service of type T.
GetService<T> method will return null if no service of type T is registered.
Whereas GetRequiredService<T> method will throw an exception if no service of type T is registered.
*/

// Instead of blindly depending on our custom service being the first registered service, we must get service name based on filter
// var hostedServiceName = app.Services.GetServices<IHostedService>().FirstOrDefault()?.GetType().Name ?? "No Service";
var hostedServiceName = app.Services.GetServices<IHostedService> ()
	.FirstOrDefault (s => s.GetType ().Namespace?.Contains ("BackgroundAndHostedServices.Services") == true)?.GetType ().Name;

var logger = app.Services.GetRequiredService<ILogger<Program>> ();
logger.LogInformation ("*** Service Registered: {service} ***", hostedServiceName);

// Configure the HTTP request pipeline.
app.MapOpenApi ();

app.UseHttpsRedirection ();

app.MapGet ("/", () =>
{
	var result = new EndpointResponse
	{
		Message = "Hello World!",
		ServiceName = hostedServiceName,
		CurrentTime = DateTime.Now.ToShortTimeString ()
	};

	return TypedResults.Ok (result);
});

app.Run ();