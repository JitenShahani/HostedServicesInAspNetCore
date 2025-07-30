<!--
# 📚 Table of Contents

- [📘 Hosted Services in ASP.NET Core](#-hosted-services-in-asp.net-core)
    - [🎯 Key Objectives](#-key-objectives)
    - [🗂️ Project Structure](#-project-structure)
    - [🔧 Hosted Service Implementation](#-hosted-service-implementation)
        - [🟢 `Services/NotifyBackgroundService.cs` - Powered by `BackgroundService`](#-servicesnotifybackgroundservice.cs-powered-by-backgroundservice)
        - [🟡 `Services/NotifyHostedService.cs` - Built on `IHostedService`](#-servicesnotifyhostedservice.cs-built-on-ihostedservice)
        - [🔵 `Services/NotifyHostedLifecycleService.cs` – Harnessing `IHostedLifecycleService`](#-servicesnotifyhostedlifecycleservice.cs-harnessing-ihostedlifecycleservice)
    - [🧱 Startup Configuration](#-startup-configuration)
        - [🧱 Service Registration](#-service-registration)
			- [🔍 Highlights](#-highlights)
		- [🧱 Custom Configuration](#-custom-configuration)
			- [🧱 Configuring `JsonOptions`](#-configuring-jsonoptions)
			- [🧱 Configuring `HostOptions`](#-configuring-hostoptions)
				- [📘 Additional Thoughts](#-additional-thoughts)
	- [📦 DTO Definition (`Dtos/EndpointResponse.cs`)](#-dto-definition-dtosendpointresponse.cs)
	- [🧠 Hosted Service Comparison & Design Considerations](#-hosted-service-comparison-design-considerations)
		- [1️⃣ Lifecycle Scope & Execution Model](#1-lifecycle-scope-execution-model)
		- [2️⃣ Practical Fit & Real-World Use Cases](#2-practical-fit-real-world-use-cases)
		- [3️⃣ Lifecycle Coordination & Developer Responsibilities](#3-lifecycle-coordination-developer-responsibilities)
		- [4️⃣ Pros and Cons of each Service Type](#4-pros-and-cons-of-each-service-type)
			- [BackgroundService](#backgroundservice)
			- [IHostedService](#ihostedservice)
			- [IHostedLifecycleService](#ihostedlifecycleservice)
		- [5️⃣ Expected Exceptions on App Shutdown](#5-expected-exceptions-on-app-shutdown)
		- [6️⃣ Execution Semantics of `BackgroundService`](#6-execution-semantics-of-backgroundservice)
			- [⚙️ Method Flow Breakdown](#-method-flow-breakdown)
			- [⚠️ Common Pitfalls to Avoid](#-common-pitfalls-to-avoid)
		- [7️⃣ `Scoped` & `Transient` Dependency Injection in Hosted Services](#7-scoped-transient-dependency-injection-in-hosted-services)
			- [🔧 Use `IServiceScopeFactory`](#-use-iservicescopefactory)
			- [📌 Sample Implementation](#-sample-implementation)
			- [🧠 Key Tips](#-key-tips)
		- [8️⃣ Honor the Cancellation Token](#8-honor-the-cancellation-token)
		- [9️⃣ Where Hosted Services Shine](#9-where-hosted-services-shine)
	- [🖥️ Service Reflection Endpoint](#-service-reflection-endpoint)
			- [💻 `GET /`](#-get-)
	- [🌐 Sample HTTP Request (`BackgroundAndHostedServices.http`)](#-sample-http-request-backgroundandhostedservices.http)
		- [🌐 Root Endpoint (`GET /`)](#-root-endpoint-get-)
			- [Sample Request](#sample-request)
			- [Sample Response](#sample-response)
	- [🔄 End-to-End Request Pipeline](#-end-to-end-request-pipeline)
	- [🚨 Common Pitfalls](#-common-pitfalls)
	- [✅ Best Practices](#-best-practices)
	- [🛡️ Hosted Service Validation Checklist](#-hosted-service-validation-checklist)
-->

# 📘 Hosted Services in ASP.NET Core

[![Microsoft.AspNetCore.OpenApi](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.OpenApi.svg?label=Microsoft.AspNetCore.OpenApi&style=flat-square&logo=Nuget)](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/)

**Note**: In ASP.NET Core, components that manage background orchestration are commonly referred to as `Hosted Services`. While the term covers several abstractions, this guide uses `Hosted Service` or `Background Service` as umbrella labels for clarity. The nuances will be unpacked later.

Behind the scenes, these services ensure your app starts gracefully, runs predictably, and shuts down cleanly. They offer a reliable mechanism for executing tasks beyond the request-response flow, coordinating initialization logic, running long-lived background processes, or ensuring graceful shutdown. These services integrate tightly with the host lifecycle, operating seamlessly alongside your primary workflows.

This repository offers a thoughtful walkthrough of ASP.NET Core’s hosted service model using practical patterns for background processing. Built with **.NET 9**, it showcases multiple service implementations, including scoped service activation techniques commonly used in background workflows.

Understanding these lifecycles is key to building resilient systems. Whether you're evolving legacy workloads or starting fresh, this reference explores **what hosted services are**, **how they differ**, and **where they shine** within modern .NET applications. You’ll also learn selection strategies and lifecycle coordination techniques for staged execution and shutdown.

👉 Let’s explore how hosted services enable reliable background execution and how ASP.NET Core helps you build it with clarity and elegance.

## 🎯 Key Objectives

This guide is crafted to illuminate hosted services in ASP.NET Core with architectural clarity and hands-on context. By exploring this repository, you'll be able to:

- ✔️ Understand how hosted services participate in ASP.NET Core’s execution lifecycle, from application startup to graceful shutdown.
- ✔️ Distinguish service types by their lifecycle hooks, startup behaviors, and execution strategies.
- ✔️ Design resilient workflows using cancellation tokens, structured delays, and error boundaries.
- ✔️ Apply graceful shutdown techniques that protect data and respect application lifecycles.
- ✔️ Configure service launch patterns, including execution order, scoped dependencies, and initialization logic.
- ✔️ Compare lifecycle strategies across service types, choosing orchestration flows that best suit reliability, precision, and maintainability.
- ✔️ Select the appropriate hosted service abstraction based on execution duration, workload characteristics, and reliability goals.

Whether refining legacy workloads or starting fresh, these objectives provide the scaffolding to help you build hosted services with confidence, clarity, and architectural intent.

## 🗂️ Project Structure

The structure of this repository has been intentionally crafted to map each hosted service to a distinct behavioral scenario, offering a clear path to explore execution rhythms, lifecycle logging, and service orchestration.

```plaintext
├── BackgroundAndHostedServices
│   ├── Dtos
│   │   └── EndpointResponse.cs             # Represents the shape of the response sent from the root endpoint
│   ├── Services
│   │   └── NotifyBackgroundService.cs      # Demonstrates long-running execution
│   │   └── NotifyHostedService.cs          # Demonstrates manual lifecycle control
│   │   └── NotifyHostedLifeCycleService.cs # Highlights advanced lifecycle hooks for staged startup/shutdown
│   ├── appsettings.json                    # Configures JSON serialization behavior
│   ├── BackgroundAndHostedServices.http    # REST client script to test local endpoint responses
│   └── Program.cs                          # Main entry point with service registration, filtering logic, and endpoint mapping
```

## 🔧 Hosted Service Implementation

This section presents the actual source code of the three hosted service types supported in ASP.NET Core. Each example highlights how the service manages lifecycle events and shutdown behavior, equipping developers with firsthand experience before diving into broader comparisons. All implementations reside in the `/Services` folder.

### 🟢 `Services/NotifyBackgroundService.cs` - Powered by `BackgroundService`

The `BackgroundService` abstract class is the most commonly used option for implementing long-running tasks in ASP.NET Core. It simplifies the hosting model by exposing a single method, `ExecuteAsync`, where your background logic resides. This pattern is ideal for scenarios such as polling, event listening, or periodic processing that occur independently of HTTP requests. It integrates seamlessly with the app’s shutdown lifecycle and supports cancellation gracefully.

The core logic is wrapped in a `try/catch` block because whenever a long-running process is triggered, whether through loops or asynchronous delays, exception handling becomes essential. If the API application is closed during execution, cancellation will be signaled, and unhandled exceptions such as `TaskCanceledException` may occur. Handling them locally or globally helps maintain clean logs and keeps the implementation focused. This example uses local exception handling to ensure that the service can exit gracefully without introducing complexity from global error handlers.

```csharp
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
```

### 🟡 `Services/NotifyHostedService.cs` - Built on `IHostedService`

Implementing `IHostedService` directly gives developers complete control over service lifecycle hooks. With this interface, you define `StartAsync` and `StopAsync` methods explicitly, allowing precise orchestration of initialization and teardown logic. This is especially useful for scenarios where your service interacts with external systems, requires deferred startup actions, or includes customized shutdown procedures.

Exception handling is introduced locally within `StartAsync` to account for cancellation during active execution, just as seen in the previous example.

```csharp
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
```

### 🔵 `Services/NotifyHostedLifeCycleService.cs` – Harnessing `IHostedLifecycleService`

This service takes full advantage of the granular lifecycle control introduced in **.NET 8**. The implementation showcases all six lifecycle stages with distinct log entries and controlled delays, offering granular control over startup and shutdown behavior. This precision is especially valuable for telemetry, dependency orchestration, and reliable cleanup where visibility across the full lifecycle is mission-critical.

`IHostedLifecycleService` expands upon `IHostedService` by fragmenting the startup and shutdown flows into three precise stages each resulting in six distinct lifecycle hooks.

During startup:

- `StartingAsync` runs before any hosted services begin.

- `StartAsync` runs when the service is initializing.

- `StartedAsync` runs once the host has fully started.

Similarly, shutdown comprises:

- `StoppingAsync` before shutdown starts,

- `StopAsync` as shutdown progresses,

- `StoppedAsync` after the host stops completely.

```csharp
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
```

Each implementation logs lifecycle events to the console for enhanced visibility, enabling developers to observe differences in startup sequencing, cancellation handling, and shutdown behavior. Together, they illustrate ASP.NET Core’s scalable service architecture, from simple loop-based execution to full-phase coordination. This approach allows hosted services to align with operational reliability, lifecycle awareness, and production-grade resilience.

## 🧱 Startup Configuration

This section outlines how hosted services are registered and configured within the ASP.NET Core application. The focus is on demonstrating lifecycle behaviors using clear service composition and minimal custom settings.

### 🧱 Service Registration

Let's explore how these services are registered and executed within the application. Each hosted service is registered to the dependency injection container using `AddHostedService<T> ()`, enabling precise lifecycle integration with the host.

Rather than running all three services simultaneously, which would result in overlapping logs and diminished clarity, this project implements a simple randomized startup strategy which intentionally activates one service per run, ensuring a clean console output and focused lifecycle behavior. This structure helps compare execution patterns, startup hooks, and shutdown handling in isolation.

```csharp
_ = Random.Shared.Next (0, 3) switch
{
	0 => builder.Services.AddHostedService<NotifyBackgroundService> (),
	1 => builder.Services.AddHostedService<NotifyHostedService> (),
	2 => builder.Services.AddHostedService<NotifyHostedLifeCycleService> (),
	_ => throw new InvalidOperationException ("Unexpected value for random")
};
```

#### 🔍 Highlights

- A single hosted service is selected randomly during startup, allowing isolated lifecycle observation per run.
- Useful for understanding behavior differences across service types without changing the source manually.
- The root endpoint confirms successful startup and enables quick testing.
- Each service type is implemented with clear separation of concerns, focusing on its unique lifecycle and execution patterns.
- Console output remains clean and focused, free from noise or concurrency clashes.

### 🧱 Custom Configuration

ASP.NET Core allows fine-tuned control over startup behavior and background service orchestration. This section explores how to configure serialization settings and host lifecycle parameters.

#### 🧱 Configuring `JsonOptions`

To ensure consistent formatting and predictable client-server communication, this project applies purposeful JSON serialization settings. These settings apply globally across all JSON responses, ensuring that the output is clean, predictable, and optimized for client consumption. Remember, these settings will be overridden by [JsonPropertyName("")] attributes in the response DTOs, allowing for specific customizations where needed.

```csharp
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
```

#### 🧱 Configuring `HostOptions`

ASP.NET Core provides `HostOptions` as a fine-grained mechanism to influence how services behave during application startup and shutdown. Though often overlooked, these settings play a crucial role in crafting resilient applications with predictable lifecycles. Here we can configure the host to start and stop services concurrently, which can significantly improve performance and reduce the time it takes for the application to start and stop.

Hosted services such as `BackgroundService`, `IHostedService`, and `IHostedLifecycleService` begin executing automatically when registered via `AddHostedService<T> ()`. However, their startup behavior can impact application readiness, especially when `StartAsync` includes blocking loops or long-running operations. While `BackgroundService` typically wraps this logic in `ExecuteAsync`, allowing the host to continue startup, services implementing `IHostedService` or `IHostedLifecycleService` may stall the host until `StartAsync` completes. To mitigate bottlenecks, this project configures `HostOptions` to start and stop services concurrently, promoting faster readiness and smoother shutdown.

```csharp
builder.Services.Configure<HostOptions> (options =>
{
	// Start all services in parallel to improve readiness.
	options.ServicesStartConcurrently = true;

	// Stop all services in parallel for quicker shutdown.
	options.ServicesStopConcurrently = true;
});
```

While this demo runs only one hosted service per cycle, enabling parallel startup and shutdown helps simulate real-world behaviors and uncover subtle lifecycle interactions. This configuration also ensures predictable teardown, even if multiple services are introduced later. Think of it as future-proofing for scale and clarity.

##### 📘 Additional Thoughts

For advanced workloads, consider adding timeout settings such as `StartupTimeout` and `ShutdownTimeout`, or control failure responses with `BackgroundServiceExceptionBehavior.Ignore` or `BackgroundServiceExceptionBehavior.StopHost`. While those aren't configured here, these options are especially valuable when lifecycle consistency is crucial such as in production-grade services handling sensitive operations or graceful recovery.

## 📦 DTO Definition (`Dtos/EndpointResponse.cs`)

This class provides a minimal but meaningful payload for the root endpoint (`GET /`). It reports the name of the activated hosted service and the current timestamp.

```csharp
public class EndpointResponse
{
	public string? Message { get; set; }
	public string? ServiceName { get; set; }
	public string? CurrentTime { get; set; }
}
```

## 🧠 Hosted Service Comparison & Design Considerations

This section builds on the service implementations explored earlier and shifts the focus toward architectural reasoning. Here, we compare the hosted service types supported in ASP.NET Core, unpack their core differences, highlight real-world use cases, and examine how they behave during application startup and shutdown. We also discuss common coordination challenges, dependency injection patterns, and scenarios where hosted services truly shine, helping developers choose the most appropriate model for their workload's intent.

### 1️⃣ Lifecycle Scope & Execution Model

Each hosted service type in ASP.NET Core defines its own lifecycle boundary and degree of host integration. This affects *when* its logic runs, *how* it participates in startup/shutdown sequencing, and *what* orchestration responsibilities the developer inherits.

- 🟢 `BackgroundService`

	- Executes its logic in `ExecuteAsync`, after the host has started.
	- Ideal for passive, self-contained loops that run until cancelled.
	- Lifecycle hooks like startup or teardown are abstracted away.

- 🟡 `IHostedService`

	- Offers direct hooks into `StartAsync` and `StopAsync`.
	- Runs logic before host readiness and during shutdown which is ideal for coordination.
	- Full control means full responsibility: logging, cancellation, cleanup.

- 🔵 `IHostedLifecycleService`

	- Provides six distinct lifecycle phases: `StartingAsync`, `StartAsync`, `StartedAsync`, `StoppingAsync`, `StopAsync`, `StoppedAsync`, as explained in the related section above.
	- Enables staged orchestration for complex startup and teardown sequences.
	- Tailored for distributed workloads and infrastructure-aware hosting.

### 2️⃣ Practical Fit & Real-World Use Cases

Each hosted service type in ASP.NET Core is designed with different operational goals in mind. While lifecycles determine *how* a service executes, this section shows *why* you'd choose one type over another, based on your workload's intent.

This section maps each service type to practical usage patterns, helping you choose the right tool for your hosting strategy.

| Service Type				| Best Fit For											| Common Scenarios																																								|
|---------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BackgroundService`		| Passive loops with minimal lifecycle control.			| - Fetch external updates via periodic polling  <br> - Deliver non-blocking notifications in the background  <br> - Consume real-time event streams like telemetry or queues	|
| `IHostedService`			| Manual lifecycle orchestration with explicit hooks.	| - Bootstrapping jobs on app start  <br> - Preloading data or warming caches <br> - Coordinated shutdown logic																	|
| `IHostedLifecycleService`	| Staged coordination across lifecycle phases.			| - Emitting logs at each lifecycle stage <br> - Dependency sequencing during startup  <br> - Teardown choreography with telemetry												|

### 3️⃣ Lifecycle Coordination & Developer Responsibilities

Hosted services vary in how they respond to cancellation, coordinate startup, and handle shutdown complexity. This section outlines the developer’s role across timing and reliability behaviors.

- 🟢 `BackgroundService`

	- Runs after host start, exits via cancellation.
	- No early lifecycle hook. Must be responsive to token signals.
	- Blocking loops or delays can prevent graceful shutdown.

- 🟡 `IHostedService`

	- Hooks before host readiness and during shutdown.
	- Manual sequencing required for multi-service coordination.
	- Cancellation, logging, and error handling must be handled explicitly.

- 🔵 `IHostedLifecycleService`

	- Breaks execution into 3 distinct startup and shutdown stages.
	- Supports precise timing.
	- Demands disciplined design for staged orchestration and clean teardown.

🕰️ **Timing Impact**: The host only starts fully after all registered services have completed their `StartingAsync` and `StartAsync` stages. Similarly, shutdown is blocked until `StoppingAsync` and `StopAsync` finish execution. Long-running operations that disregard `Concurrency configuration` or `CancellationToken`, can prevent the app from launching or result in abrupt termination.

### 4️⃣ Pros and Cons of each Service Type

This section offers a detailed evaluation of all three service types in ASP.NET Core, including their advantages, limitations, and architectural implications. Use these traits to guide your selection based on your app’s coordination, orchestration, and background processing needs.

#### `BackgroundService`

- ✅ **Pros**
  - Simple to implement with minimal boilerplate.
  - Encourages loop-oriented execution (e.g., polling, queue listening) via `ExecuteAsync (...)` method.
  - Supports cancellation automatically through `CancellationToken`, promoting responsive termination.
  - Great fit for lightweight jobs that don't require startup orchestration.

- ⚠️ **Cons**
  - No lifecycle hooks before or after host start. Lacks control over coordinated startup/shutdown.
  - Limited suitability for phase-sensitive tasks or readiness signaling.
  - If using a loop within `ExecuteAsync (...)`, cancellation must be handled explicitly to prevent shutdown delays. Blocking calls or ignoring `CancellationToken` can lead to stalled termination. For non-looping tasks, ensure the token is still observed if the task may run long or hold resources.
  - Difficult to integrate with infrastructure timing (e.g., warm-up, health probes, staged teardown).

#### `IHostedService`

- ✅ **Pros**
  - Explicit entry and exit points for background tasks. Improves clarity and testability.
  - Ideal for startup routines, initialization logic, and one-off bootstrapping.
  - Can support both persistent and discrete workflows using custom loops or timers.
  - Easier to debug due to method-based lifecycle control.

- ⚠️ **Cons**
  - Requires manual handling of cancellation, looping, and error propagation.
  - Lacks lifecycle phase granularity. Cannot react to precise startup or shutdown stages.
  - Easy to overlook graceful termination patterns in complex flows.
  - Slightly more verbose for simple scenarios compared to `BackgroundService`.

#### `IHostedLifecycleService`

- ✅ **Pros**
  - Granular control over lifecycle phases.
  - Enables coordination with external systems via readiness probes, health checks, and telemetry logging.
  - Excellent for structured cleanup and graceful termination e.g., flush telemetry before sockets drop.
  - Tailored for cloud-native and infrastructure-heavy apps that require synchronized timing.

- ⚠️ **Cons**
  - More verbose and cognitively demanding to implement correctly. Each method has specific intent and timing.
  - Easy to misuse or duplicate logic across lifecycle stages if responsibilities aren’t clearly defined.
  - Potentially over-engineered for lightweight scenarios.
  - Requires deeper understanding of host behavior and timing orchestration.

### 5️⃣ Expected Exceptions on App Shutdown

During shutdown, hosted services including those based on `BackgroundService` may encounter expected or benign exceptions triggered by cancellation. As the application begins terminating, background tasks might still be mid-loop, awaiting timeouts, interacting with external resources, or performing I/O. These operations may not exit instantly, and the resulting runtime signals often reflect cooperative interruption rather than failure.

These are not failures in the traditional sense. They're byproducts of work being halted mid-flight, often because shutdown signals were honored, and the app exited while tasks were still in transition.

- Log such events with a calm, contextual tone to indicate they occurred during a cooperative termination phase.

- Don’t treat them as errors unless they result in unreleased resources, blocked threads, or data loss.

- Design loops and long-running operations to observe shutdown signals early and exit intentionally.

> ℹ️ Exception logs that appear during shutdown, especially those involving cancellation tokens, task timeouts, or `TaskCanceledException`, usually indicate that background operations were interrupted as part of a graceful termination. These logs should be seen as expected behavior rather than failure.

### 6️⃣ Execution Semantics of `BackgroundService`

The execution backbone of a `BackgroundService` is its overridden `ExecuteAsync (...)` method. This method begins only after the application host has started and continues until cancellation is signaled during shutdown.

#### ⚙️ Method Flow Breakdown

```csharp
public class WorkerService : BackgroundService
{
	private readonly ILogger<WorkerService> _logger;

	public WorkerService (ILogger<WorkerService> logger)
		=> _logger = logger;

	protected override async Task ExecuteAsync (CancellationToken stoppingToken)
	{
		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// ✅ Your recurring logic goes here
				await DoWorkAsync ();

				// 🔄 Optional delay pattern to throttle execution
				await Task.Delay (TimeSpan.FromSeconds (10), stoppingToken);
			}

			// 🧹 Optionally clean up before exiting
			await CleanupAsync ();
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation ("Task was canceled gracefully...");
		}
	}
}
```

- The `ExecuteAsync` method should remain active until cancellation is requested or the background task finishes in a rare one-off scenario.
- If this method exits too early, the host interprets the service as finished even if cancellation has not occurred.
- Long-running logic should always observe the stopping token to allow for graceful shutdown

#### ⚠️ Common Pitfalls to Avoid

Common mistakes in `ExecuteAsync` often stem from assumptions about method scope or host lifetime.

| Pitfall								| Why It Matters																			|
|---------------------------------------|-------------------------------------------------------------------------------------------|
| Returning from `ExecuteAsync` early	| Stops execution silently and makes the service appear healthy but non-functional.			|
| Ignoring `stoppingToken`				| Prevents graceful shutdown and might cause resource leakage or delayed termination.		|
| Infinite loops without throttle		| Can hog CPU and starve system resources. Always add a delay.								|
| Async void methods inside				| Breaks exception tracking. Always use async Task.											|

> 💡 Bonus Tip: You can wire diagnostics, telemetry, or heartbeat checks inside the loop for real-time status visibility. That way, your service isn’t just running. It’s traceable and observable.

### 7️⃣ `Scoped` & `Transient` Dependency Injection in Hosted Services

Hosted services in ASP.NET Core are registered as `Singleton` by default. Avoid injecting `Scoped` or `Transient` services directly. Instead, use `IServiceScopeFactory` to resolve both within a manually created `Scope` during background execution.

#### 🔧 Use `IServiceScopeFactory`

Inject `IServiceScopeFactory` into your hosted service class and resolve services inside a controlled `Scope` block. This ensures proper disposal for `Scoped` services and consistent behavior for `Transient` ones.

#### 📌 Sample Implementation

```csharp
public class WorkerService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<WorkerService> _logger;

	public WorkerService (IServiceScopeFactory scopeFactory, ILogger<WorkerService> logger)
		=> (_scopeFactory, _logger) = (scopeFactory, logger);

	protected override async Task ExecuteAsync (CancellationToken stoppingToken)
	{
		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = _scopeFactory.CreateScope ();

				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext> ();		// Scoped
				var notifier = scope.ServiceProvider.GetRequiredService<ITransientNotifier> ();	// Transient

				await dbContext.CleanupStaleRecordsAsync ();	// Sample scoped operation
				notifier.NotifyCleanupCompleted (); // Sample transient operation

				await Task.Delay (TimeSpan.FromMinutes (5), stoppingToken);
			}
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation ("Task was canceled gracefully...");
		}
	}
}
```

#### 🧠 Key Tips

- Understanding service lifetimes isn’t just theoretical. It directly shapes performance, resilience, and shutdown behavior.

- `Scoped` services are tied to the `Scope` lifetime and disposed when the `Scope` ends.

- `Transient` services are instantiated fresh with each resolution and ideal for stateless, short-lived tasks.

- `Transient` services implementing `IDisposable` should be resolved through the `Scope` so the container handles disposal.

- Avoid caching `Scoped` or `Transient` instances across loop iterations. Always resolve them inside the `Scope`.

> 💡 This approach keeps your background service lean while respecting dependency injection lifetime rules. A perfect blend of safety and flexibility.

### 8️⃣ Honor the Cancellation Token

Hosted services, including those based on `BackgroundService`, must respond promptly to shutdown signals like `Ctrl+C`, `SIGTERM`, or application termination. Failing to do so leads to lingering threads, incomplete cleanup, or unpredictable behavior at shutdown.

- **Respect task boundaries**. For `BackgroundService`, use the `CancellationToken` passed to `ExecuteAsync` to halt operations gracefully. For hosted services, monitor the token passed to `StopAsync` or propagate it to long-running operations. This allows your service to participate in shutdowns, restarts, and configuration reloads in a cooperative manner.

- **Check before you wait**. Always test `stoppingToken.IsCancellationRequested` (or equivalent) in loop conditions and inject it into awaited calls like `Task.Delay(...)` or I/O methods. Injecting the token avoids unnecessary delays during shutdown and ensures responsive exits.

- **Exit cleanly**. Release resources, commit final operations, and avoid blocking calls or infinite loops. Let the host terminate the service without resistance or delay.

- **Coordinate with the host lifecycle**. In `BackgroundService`, the host passes the cancellation token into `ExecuteAsync` and expects that method to honor it directly. There’s no separate `StopAsync` override. In custom `IHostedService` implementations, the host passes cancellation into `StopAsync`, offering a dedicated hook for graceful shutdown logic. In both patterns, cancellation signaling is your architectural handshake with the host. Respecting it ensures clean coordination and predictable termination.

> 💡 Cancellation is not an afterthought. It’s the backbone of clean service orchestration. Treat it like an architectural contract between your code and the host.

### 9️⃣ Where Hosted Services Shine

Hosted services, whether implemented directly via `IHostedService` or through the `BackgroundService` base class, are purpose-built for background work that is persistent, reactive, and aware of shutdown signals. They integrate cleanly into the host's lifecycle, making them ideal for:

- **Queue processing**. Continuously pull and process messages from external brokers like `RabbitMQ`, `Azure Service Bus`, or `Kafka`. Use `BackgroundService` for looping tasks, or `IHostedService` for custom orchestration.

- **File watching**. Monitor folders using `FileSystemWatcher` for triggers like new uploads, deletions, or updates. Perfect for ingestion pipelines or sync routines.

- **Integration polling**. Periodically query external APIs, legacy systems, or partner endpoints to detect updates, sync state, or push notifications. Use `Task.Delay(...)` or timer-based loops responsibly with cancellation support built in.

- **Graceful cleanup during shutdown**. Flush telemetry, commit audit logs, release held resources, and wrap up final actions. Hosted services excel here thanks to the host-coordinated shutdown via `StopAsync`.

> 💡 Hosted services aren’t just background threads. They’re orchestrated participants in your app’s lifecycle. When used intentionally, they add reliability, clarity, and runtime empathy to your architecture.

## 🖥️ Service Reflection Endpoint

This demo presents a single Minimal API endpoint, purpose-built and intentionally positioned to showcase hosted service activity. It functions as a runtime probe that confirms service discoverability and operational flow. Behind its minimalist facade, it reflects key patterns in dependency resolution, structured logging, and background service coordination, offering developers a focused entry point into ASP.NET Core’s hosting ecosystem.

### 💻 `GET /`

This endpoint offers a lightweight diagnostics probe to confirm that the application and its hosted services are wired up correctly.

```csharp
var hostedServiceName = app.Services.GetServices<IHostedService> ()
	.FirstOrDefault (s => s.GetType ().Namespace?.Contains ("BackgroundAndHostedServices.Services") == true)?.GetType ().Name;

var logger = app.Services.GetRequiredService<ILogger<Program>> ();
logger.LogInformation ("*** Service Registered: {service} ***", hostedServiceName);

app.MapGet ("/", () =>
{
	var result = new EndpointResponse
	{
		Message = "Hello World!",
		ServiceName = hostedServiceName,
		CurrentTime = DateTime.Now.ToShortTimeString()
	};

	return TypedResults.Ok (result);
});
```

- In this demo, `IHostedService` may include over 150 services, most registered internally by the framework. To avoid ambiguity and fragile assumptions, we refrain from using `GetRequiredService<T> ()`, which disregards registration order and throws if the type isn’t present.

- Instead, `GetServices<T> ()` is used alongside a LINQ filter targeting a known namespace. This ensures deterministic selection of the custom service, regardless of where or when it was registered. Even if system services are added via `builder.Services...`.

- Rather than calling `FirstOrDefault()` directly on the full service list risking retrieval of a system-registered service, we filter by namespace first. This ensures our custom service is selected deterministically, even if additional system services are configured via `builder.Services...`.

> ✅ Even minimalist endpoints can reflect architectural intent. By resolving and logging services explicitly, this pattern builds confidence in hosting behavior while remaining lightweight and discoverable.

## 🌐 Sample HTTP Request (`BackgroundAndHostedServices.http`)

📄 Request samples sourced from [`BackgroundAndHostedServices.http`](/BackgroundAndHostedServices/BackgroundAndHostedServices.http)

This section provides concrete request/response examples for the root endpoint. It reinforces DTO structure and expected runtime behavior, while staying decoupled from implementation details.

### 🌐 Root Endpoint (`GET /`)

#### Sample Request

```http
@HostAddress = http://localhost:5106

GET {{HostAddress}}/
Content-Type: none

###
```

#### Sample Response

The response varies depending on which custom service is currently active.

`NotifyBackgroundService`

```json
{
	"Message": "Hello World!",
	"ServiceName": "NotifyBackgroundService",
	"CurrentTime": "10:39 PM"
}
```

`NotifyHostedService`

```json
{
	"Message": "Hello World!",
	"ServiceName": "NotifyHostedService",
	"CurrentTime": "10:38 PM"
}
```

`NotifyHostedLifecycleService`

```json
{
	"Message": "Hello World!",
	"ServiceName": "NotifyHostedLifeCycleService",
	"CurrentTime": "10:41 PM"
}
```

## 🔄 End-to-End Request Pipeline

Let’s trace how the system responds to a client request by walking through service registration, internal execution, and final JSON delivery.

```
[1] 📨 Client sends HTTP request
	└── Example:
		- GET /

	✅ A stateless call issued to the root endpoint (`GET /`) via browser, REST client, or .http file.

[2] 🔧 Service is registered
	└── Registration occurs at application startup inside Program.cs

	✅ This service is not dynamic per request. It is resolved once by the DI container and persists for the application's lifetime.
	✅ Swapping between NotifyBackgroundService, NotifyHostedService, or NotifyHostedLifeCycleService requires application restart.
	✅ The registered class determines:
		- The behavior during app startup.
		- The value of ServiceName in the final response.

[3] 📥 Hosted service is resolved at startup
	└── When the IHost is built, the registered service is instantiated and scheduled internally.
	└── Execution begins via lifecycle methods:
		- ExecuteAsync () → BackgroundService
		- StartAsync () & StopAsync () → IHostedService
		- StartingAsync (), StartAsync (), StartedAsync () / StoppingAsync (), StopAsync (), StoppedAsync () with hooks → IHostedLifecycleService

	✅ These run on background threads and can log, initialize resources, or influence application readiness.
	✅ They do not directly respond to HTTP requests.

[4] 🛂 Minimal API endpoint receives the request
	└── Endpoint is mapped using app.MapGet("/", ...)
	└── Returns a JSON object containing:
		- Message: "Hello World!"
		- ServiceName: <Name of the registered service>
		- CurrentTime: <Current server time>

[5] 📤 Server returns appropriate response
	◀── ✅ 200 Ok

	✅ The response remains fixed in shape and only the ServiceName & CurrentTime changes based on what was registered at startup.
```

> 📚 This walkthrough clarifies how hosted services operate independently of request-response logic, yet still shape the runtime experience. Each step from registration to background execution affects what the root endpoint reveals, without coupling service behavior to HTTP flow. It's a model of predictability through composition.

## 🚨 Common Pitfalls

Uncover subtle mistakes that can derail hosted service behavior. These reminders help ensure predictable execution and clean lifecycle management.

- ❌ **Assuming hosted services require manual execution triggers**

	- All hosted service types (`BackgroundService`, `IHostedService`, and `IHostedLifecycleService`) are executed automatically when registered via `AddHostedService<T> ()`.

	- The ASP.NET Core host waits for the registered service’s `StartAsync ()` method to complete before it considers startup finished.

	- If `StartAsync ()` includes long-running or blocking logic, it can delay full application readiness even though the service is already executing.

- ❌ **Misunderstanding service lifetime**

	- Hosted services are registered as singletons by default. This behavior is enforced by the framework.

	- Attempting to inject `Scoped` or `Transient` services into a hosted service will result in exceptions.

	- Developers don’t configure the lifetime manually. ASP.NET Core ensures `Singleton` registration when using `AddHostedService<T> ()`.


- ❌ **Expecting request/response logic inside hosted service**

	- Hosted services run independently of HTTP traffic.

	- They cannot access `HttpContext`, nor do they respond per request.

	- Think of them as background orchestrators and not endpoint participants.

- ❌ **Over complicating lifecycle hooks**

	- `IHostedLifecycleService` offers granular startup and shutdown callbacks.

	- Only use lifecycle methods where initialization, cleanup, or readiness sequencing truly require them.

- ❌ **Forgetting cancellation tokens**

	- Hosted services must observe `CancellationToken` to support graceful shutdown.

	- Ignoring token handling in long-running loops or background tasks can prevent proper cleanup and lead to forceful termination during shutdown.

- ❌ **Improper exception handling**

	- Unhandled exceptions in `StartAsync ()` or `ExecuteAsync ()` will crash the host or trigger restarts depending on the hosting environment.

	- Log and fail gracefully, especially around startup validation, connectivity, or critical background operations.

- ❌ **Assuming concurrency safety**

	- Hosted services are singleton by default, but they aren't magically thread-safe.

	- Protect shared state using locks or concurrency primitives if executing async workflows or shared caching logic.

- ⚠️ **Overlooking host shutdown timeouts**

	- The host calls `StopAsync ()` on each hosted service during shutdown and waits for completion but only up to a configurable timeout (`HostOptions.ShutdownTimeout`).

	- After the timeout, the host cancels the provided token and proceeds with termination, whether the service has finished or not.

	- Observing the `CancellationToken` during shutdown is the key to cooperative cleanup. Services that ignore it may face abrupt termination and resource loss.

## ✅ Best Practices

The following practices focus on refining behavior, lifecycle coordination, and architectural clarity.

- ✅ **Understand Hosted Service Types**

	- `BackgroundService`: Starts automatically when registered. Commonly used for looping tasks, but can also support one-time or event-driven operations.

	- `IHostedService`: Offers flexible background execution without prescribing a pattern like `BackgroundService`. Commonly used when more direct control over startup behavior is needed.

	- `IHostedLifecycleService`: Adds hooks like `StartingAsync ()` and `StartedAsync ()`. Helpful for readiness staging.

- ✅ **Handle cancellation cooperatively**

	- All hosted service lifecycle methods receive a `CancellationToken` as their first parameter.

	- Honor it in background loops and async tasks to avoid hangs or abrupt terminations.

- ✅ **Keep constructors lightweight**

	- Avoid business logic or resource access in constructors.

	- Use `StartAsync()` or lifecycle hooks for time-consuming operations.

- ✅ **Separate Responsibilities with Internal Services**

	- Delegate long-running logic (e.g., polling, scheduling) to injected services.

	- This improves testability, modularity, and keeps your hosted service focused on lifecycle coordination.

- ✅ **Log responsibly**

	- Inject `ILogger<T>` for structured, contextual logging.

	- Avoid static or global loggers, and never rely on `Console.WriteLine ()` especially in async or cloud scenarios.

- ✅ **Avoid coupling with request pipeline**

	- Hosted services operate independently of incoming HTTP requests.

	- Don’t access `HttpContext`, routing data, or endpoint metadata. These belong in controller or middleware scope.

- ✅ **Use Lifecycle Hooks intentionally**

	- Reach for `IHostedLifecycleService` only when you need staged startup.

	- For simpler loops or tasks, `BackgroundService` is often the cleanest solution.

- ✅ **Isolate critical operations**

	- Long-running tasks should include retry policies, exception boundaries, and circuit breakers if interacting with unstable systems.

	- This ensures hosted services don’t destabilize the host due to transient faults.

## 🛡️ Hosted Service Validation Checklist

- [ ] Is your hosted service registered via `AddHostedService<T> ()`?
- [ ] Are all injected dependencies singleton-safe?
- [ ] Are you observing `CancellationToken` in long-running operations?
- [ ] Are lifecycle methods implemented only when needed?
- [ ] Have you handled exceptions in all lifecycle methods?

Hosted services may run in the background, but understanding them brings clarity to the foreground. From graceful lifecycles to mindful shutdowns, these patterns shape more than just execution.They shape how we think about building resilient systems.

---

**_🧭 Stay Curious. Build Thoughtfully._**
