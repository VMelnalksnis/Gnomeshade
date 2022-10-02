// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Desktop.Authentication;
using Gnomeshade.Desktop.Views;
using Gnomeshade.WebApi.Client;

using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NodaTime;

using Serilog;

namespace Gnomeshade.Desktop;

/// <inheritdoc />
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class App : Application
{
	private static readonly ProductInfoHeaderValue _userAgent;

	private readonly IServiceProvider _serviceProvider;

	static App()
	{
		var assemblyName = typeof(App).Assembly.GetName();
		var assemblyShortName = assemblyName.Name ?? assemblyName.FullName.Split(',').First();
		_userAgent = new(assemblyShortName, assemblyName.Version?.ToString());
	}

	/// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
	public App()
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", true)
			.AddJsonFile(UserConfigurationWriter.Filepath, true, true)
			.Build();

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddLogging(builder => builder.AddSerilog());

		serviceCollection
			.AddGnomeshadeOptions(configuration)
			.AddSingleton<UserConfigurationWriter>()
			.AddHttpClient<UserConfigurationValidator>(client => client.DefaultRequestHeaders.UserAgent.Add(_userAgent));

		serviceCollection
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb)
			.AddSingleton<IActivityService, ActivityService>()
			.AddSingleton<IBrowser, SystemBrowser>(provider =>
			{
				var protocolHandler = provider.GetRequiredService<IGnomeshadeProtocolHandler>();
				var options = provider.GetRequiredService<IOptionsMonitor<OidcOptions>>().CurrentValue;
				return new(protocolHandler, options.SigninTimeout);
			});

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			serviceCollection
				.AddSingleton<IGnomeshadeProtocolHandler, WindowsProtocolHandler>()
				.AddSingleton<ICredentialStorageService, WindowsCredentialStorageService>();
		}
		else
		{
			throw new PlatformNotSupportedException($"{RuntimeInformation.OSDescription} is not supported");
		}

		serviceCollection.AddTransient<OidcClient>(provider =>
		{
			var options = provider.GetRequiredService<IOptionsMonitor<OidcOptions>>().CurrentValue.ToOidcClientOptions();
			options.Browser = provider.GetRequiredService<IBrowser>();
			options.HttpClientFactory = _ => provider.GetRequiredService<HttpClient>();
			options.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();
			return new(options);
		});

		serviceCollection.AddGnomeshadeClient(configuration);

		serviceCollection
			.AddTransient<IAuthenticationService, AuthenticationService>()
			.AddSingleton<ViewLocator<App>>()
			.AddSingleton<IDialogService, DialogService>()
			.AddSingleton<MainWindowViewModel>();

		_serviceProvider = serviceCollection.BuildServiceProvider();
	}

	/// <inheritdoc />
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	/// <inheritdoc />
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>(),
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	/// <inheritdoc />
	protected override void LogBindingError(AvaloniaProperty property, Exception exception)
	{
		Log.Error(exception, "Failed to bind property {PropertyName} from owner type {OwnerTypeName}", property.Name, property.OwnerType.Name);
		base.LogBindingError(property, exception);
	}
}
