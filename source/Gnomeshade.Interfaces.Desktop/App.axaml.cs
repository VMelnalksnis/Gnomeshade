// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Authentication;
using Gnomeshade.Interfaces.Desktop.Configuration;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;

using IdentityModel.OidcClient;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NodaTime;

using Serilog;

namespace Gnomeshade.Interfaces.Desktop;

/// <inheritdoc />
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="App"/> class.
	/// </summary>
	public App()
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", true)
			.AddJsonFile("appsettings.user.json", true)
			.AddUserSecrets<App>()
			.Build();

		var serviceCollection = new ServiceCollection();
		serviceCollection
			.AddOptions<OidcClientOptions>()
			.Bind(configuration.GetSection("Oidc"))
			.ValidateDataAnnotations()
			.ValidateOnStart();
		serviceCollection
			.AddOptions<GnomeshadeOptions>()
			.Bind(configuration.GetSection(GnomeshadeOptions._sectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		serviceCollection.AddLogging(builder => builder.AddSerilog());

		serviceCollection
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb);

		serviceCollection.AddSingleton<OidcClient>(provider =>
		{
			var options = provider.GetRequiredService<IOptions<OidcClientOptions>>().Value;
			var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
			options.Browser = new SystemBrowser(options.RedirectUri);
			options.HttpClientFactory = _ => provider.GetRequiredService<HttpClient>();
			options.LoggerFactory = loggerFactory;
			return new(options);
		});
		serviceCollection.AddHttpClient();
		serviceCollection.AddHttpClient<IGnomeshadeClient, GnomeshadeClient>(nameof(GnomeshadeClient), (provider, client) =>
		{
			var gnomeshadeOptions = provider.GetRequiredService<IOptionsSnapshot<GnomeshadeOptions>>();
			client.BaseAddress = gnomeshadeOptions.Value.BaseAddress;
			client.DefaultRequestVersion = HttpVersion.Version30;
		});

		serviceCollection
			.AddSingleton<IGnomeshadeClient, GnomeshadeClient>(provider =>
			{
				var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
				var httpClient = httpClientFactory.CreateClient(nameof(GnomeshadeClient));
				return new(httpClient);
			})
			.AddSingleton<IAuthenticationService, AuthenticationService>()
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
