// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Desktop.Authentication;
using Gnomeshade.Desktop.Views;
using Gnomeshade.WebApi.Client;

using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NodaTime;

using Serilog;

namespace Gnomeshade.Desktop;

/// <inheritdoc />
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public sealed class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
	public App()
		: this([])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
	/// <param name="args">The command line arguments with which the app was launched.</param>
	public App(string[] args)
	{
		var configurationBuilder = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", true)
			.AddJsonFile(UserConfigurationWriter.Filepath, true, true);

		if (args is [var arg, ..] &&
			Uri.TryCreate(arg, UriKind.Absolute, out var uri) &&
			HttpUtility.ParseQueryString(uri.Query).Get("baseAddress") is { } baseAddress)
		{
			configurationBuilder.AddInMemoryCollection(new KeyValuePair<string, string?>[]
			{
				new($"Gnomeshade:{nameof(GnomeshadeOptions.BaseAddress)}", baseAddress),
			});
		}

		var configuration = configurationBuilder.Build();

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddLogging(builder => builder
			.ClearProviders()
			.AddSerilog(SerilogConfiguration.CreateLogger(configuration), true));

		serviceCollection.AddGnomeshadeOptions(configuration);

		serviceCollection
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb)
			.AddSingleton<Lazy<IManagedNotificationManager>>(_ => new(() =>
			{
				if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: not null } lifetime)
				{
					throw new InvalidOperationException("Failed to get current window for notification manager");
				}

				return new WindowNotificationManager(lifetime.MainWindow);
			}))
			.AddSingleton<IActivityService, ActivityService>()
			.AddSingleton<IBrowser, SystemBrowser>(provider =>
			{
				var protocolHandler = provider.GetRequiredService<IGnomeshadeProtocolHandler>();
				var options = provider.GetRequiredService<IOptionsMonitor<UserConfiguration>>().CurrentValue.Oidc ?? new();
				return new(protocolHandler, options.SigninTimeout);
			})
			.AddSingleton<IGnomeshadeProtocolHandler, GnomeshadeProtocolHandler>();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			serviceCollection.AddSingleton<ICredentialStorageService, WindowsCredentialStorageService>();
		}
		else
		{
			serviceCollection.AddSingleton<ICredentialStorageService, StubbedCredentialStorageService>();
		}

		serviceCollection.AddTransient<OidcClient>(provider =>
		{
			var oidcOptions = provider.GetRequiredService<IOptionsMonitor<UserConfiguration>>().CurrentValue.Oidc;
			if (oidcOptions?.Authority is null)
			{
				return new NullOidcClient();
			}

			var options = oidcOptions.ToOidcClientOptions();
			options.Browser = provider.GetRequiredService<IBrowser>();
			options.HttpClientFactory = _ => provider.GetRequiredService<HttpClient>();
			options.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();
			return new(options);
		});

		serviceCollection.AddGnomeshadeClient(configuration);
		serviceCollection.AddSingleton<IGnomeshadeClient, DesignTimeGnomeshadeClient>(); // todo remove

		serviceCollection
			.AddViewModels()
			.AddTransient<IAuthenticationService, AuthenticationService>()
			.AddSingleton<ViewLocator<App>>()
			.AddSingleton<IDialogService, DialogService>();

		_serviceProvider = serviceCollection.BuildServiceProvider();
	}

	/// <inheritdoc />
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);

		LiveCharts.Configure(settings => settings
			.AddSkiaSharp()
			.AddDefaultMappers()
			.AddDarkTheme());
	}

	/// <inheritdoc />
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
			desktop.ShutdownRequested += viewModel.OnShutdownRequested;
			desktop.MainWindow = new MainWindow { DataContext = viewModel };
		}

		base.OnFrameworkInitializationCompleted();
	}
}
