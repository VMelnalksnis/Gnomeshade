// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Wizard for configuring settings needed to log in.</summary>
public sealed partial class ConfigurationWizardViewModel : ViewModelBase
{
	private readonly AuthenticationConfigurationViewModel _authenticationConfigurationViewModel;

	/// <summary>Gets the current step viewmodel.</summary>
	[Notify(Setter.Private)]
	private ConfigurationViewModel _current;

	/// <summary>Initializes a new instance of the <see cref="ConfigurationWizardViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeConfigurationViewModel">Gnomeshade API configuration viewmodel.</param>
	/// <param name="authenticationConfigurationViewModel">Authentication configuration viewmodel.</param>
	public ConfigurationWizardViewModel(
		IActivityService activityService,
		GnomeshadeConfigurationViewModel gnomeshadeConfigurationViewModel,
		AuthenticationConfigurationViewModel authenticationConfigurationViewModel)
		: base(activityService)
	{
		_authenticationConfigurationViewModel = authenticationConfigurationViewModel;

		gnomeshadeConfigurationViewModel.Updated += GnomeshadeConfigurationViewModelOnUpdated;
		_authenticationConfigurationViewModel.Updated += AuthenticationConfigurationViewModelOnUpdated;

		_current = gnomeshadeConfigurationViewModel;
	}

	/// <summary>Invoked when the wizard has been completed.</summary>
	public event EventHandler? Completed;

	/// <summary>Gets a value indicating whether <see cref="Current"/> step can be skipped.</summary>
	public bool CanSkip => Current is AuthenticationConfigurationViewModel;

	/// <summary>Skips the <see cref="Current"/> step.</summary>
	/// <exception cref="InvalidOperationException"><see cref="CanSkip"/> is not <c>true</c>.</exception>
	public void Skip()
	{
		if (!CanSkip)
		{
			throw new InvalidOperationException();
		}

		if (Current is AuthenticationConfigurationViewModel)
		{
			AuthenticationConfigurationViewModelOnUpdated(this, EventArgs.Empty);
		}
	}

	private void GnomeshadeConfigurationViewModelOnUpdated(object? sender, EventArgs e)
	{
		Current = _authenticationConfigurationViewModel;
	}

	private void AuthenticationConfigurationViewModelOnUpdated(object? sender, EventArgs e)
	{
		Completed?.Invoke(this, EventArgs.Empty);
	}
}
