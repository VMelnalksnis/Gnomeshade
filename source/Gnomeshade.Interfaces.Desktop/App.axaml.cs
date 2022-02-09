// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Desktop.Authentication;
using Gnomeshade.Interfaces.Desktop.ViewModels;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using VMelnalksnis.OAuth2;
using VMelnalksnis.OAuth2.Keycloak;

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
			.AddUserSecrets<MainWindowViewModel>()
			.Build();

		var serviceCollection = new ServiceCollection();
		serviceCollection
			.AddOptions<KeycloakOAuth2ClientOptions>()
			.Bind(configuration.GetSection("Oidc:Keycloak"))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		serviceCollection.AddLogging(builder => builder.AddSerilog());

		serviceCollection.AddHttpClient<IOAuth2Client, KeycloakOAuth2Client>();
		serviceCollection.AddHttpClient<IGnomeshadeClient, GnomeshadeClient>();

		serviceCollection
			.AddSingleton<IGnomeshadeClient, GnomeshadeClient>()
			.AddSingleton<IOAuth2Client, KeycloakOAuth2Client>()
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
