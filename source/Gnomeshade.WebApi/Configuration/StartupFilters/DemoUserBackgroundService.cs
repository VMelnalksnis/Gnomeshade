// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Services;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;

namespace Gnomeshade.WebApi.Configuration.StartupFilters;

internal sealed class DemoUserBackgroundService : BackgroundService
{
	private readonly IHostApplicationLifetime _lifetime;
	private readonly IServiceProvider _serviceProvider;
	private readonly Lazy<HttpMessageHandler> _handler;

	public DemoUserBackgroundService(IHostApplicationLifetime lifetime, IServiceProvider serviceProvider, Lazy<HttpMessageHandler>? handler = null)
	{
		_handler = handler ?? new(() => new SocketsHttpHandler());
		_lifetime = lifetime;
		_serviceProvider = serviceProvider;
	}

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!await WaitForAppStartup(stoppingToken))
		{
			return;
		}

		using var scope = _serviceProvider.CreateScope();
		var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
		if (configuration.GetChildren().Any(section => section.Key is "GNOMESHADE_DEMO"))
		{
			await CreateDemoUser(scope.ServiceProvider);
		}
	}

	private async Task<bool> WaitForAppStartup(CancellationToken stoppingToken)
	{
		var startedSource = new TaskCompletionSource();
		var cancelledSource = new TaskCompletionSource();

		await using var reg1 = _lifetime.ApplicationStarted.Register(() => startedSource.SetResult());
		await using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

		var completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task);

		// If the completed tasks was the "app started" task, return true, otherwise false
		return completedTask == startedSource.Task;
	}

	private async Task CreateDemoUser(IServiceProvider services)
	{
		var repository = services.GetRequiredService<CounterpartyRepository>();
		var counterparties = await repository.GetAllAsync();

		if (counterparties.SingleOrDefault(counterparty => counterparty.NormalizedName is "DEMO") is not null)
		{
			return;
		}

		var registrationService = services.GetRequiredService<UserRegistrationService>();

		var registerResult = await registrationService.RegisterUser("demo", "Demo", "Demo1!");
		if (!registerResult.Succeeded)
		{
			throw new("Failed to register demo user");
		}

		// The default address is not provided by the feature, so need to set it manually
		var serverAddress = services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault() ?? "http://localhost:5000/";
		var baseAddress = new UriBuilder(serverAddress) { Host = "localhost", Path = "/api/" }.Uri;

		var clock = services.GetRequiredService<IClock>();
		var dateTimeZoneProvider = services.GetRequiredService<IDateTimeZoneProvider>();

		// todo This probably needs to be reworked by not using an HttpClient to call the API from the API
		var client = new GnomeshadeClient(
			new(new TokenDelegatingHandler(new(clock), new(dateTimeZoneProvider), new NullOidcClient())
			{
				InnerHandler = _handler.Value,
			}) { BaseAddress = baseAddress },
			new(dateTimeZoneProvider));

		var loginResult = await client.LogInAsync(new() { Username = "demo", Password = "Demo1!" });
		if (loginResult is FailedLogin)
		{
			throw new("Failed to log in with demo user");
		}

		var service = new DemoUserService(client, clock, dateTimeZoneProvider);
		await service.GenerateData();
	}
}
