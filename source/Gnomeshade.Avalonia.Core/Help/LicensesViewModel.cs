// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Commands;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Help;

/// <summary>Information about application licensing.</summary>
public sealed partial class LicensesViewModel : ViewModelBase
{
	private static readonly DataGridGroupDescription _groupDescription =
		new DataGridTypedGroupDescription<PackageInfo, Uri>(info => info.PackageProjectUrl);

	private static readonly DataGridComparerSortDescription _sortDescription =
		new(new PackageInfoComparer(), ListSortDirection.Ascending);

	/// <summary>Gets a collection of all dependencies.</summary>
	[Notify(Setter.Private)]
	private DataGridItemCollectionView<PackageInfo> _packages;

	/// <summary>Gets or sets the selected dependency.</summary>
	[Notify]
	private PackageInfo? _selected;

	/// <summary>Initializes a new instance of the <see cref="LicensesViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="commandFactory">Service for creating commands.</param>
	public LicensesViewModel(IActivityService activityService, ICommandFactory commandFactory)
		: base(activityService)
	{
		_packages = new(Array.Empty<PackageInfo>());

		Description = """
Gnomeshade is licensed under the GNU Affero General Public License 3.0 (AGPL 3.0).
Below is a list of all packages and their licenses, grouped by projects, which Gnomeshade depends on.
(Note: your copy of this product may not contain code covered by one or more of the licenses list here, depending on the exact product and version you choose.)
""";

		OpenProject = commandFactory.Create(OpenSelected, () => TryGetUrl(out _), "Opening project information");
		OpenLicense = commandFactory.Create(OpenSelectedLicense, () => TryGetLicenseUrl(out _), "Opening license");

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the <see cref="DataGridCollectionView"/> of <see cref="Packages"/>.</summary>
	public DataGridCollectionView DataGridView => Packages;

	/// <summary>Gets the licenses description text.</summary>
	public string Description { get; }

	/// <summary>Gets a command for opening the selected project information.</summary>
	public CommandBase OpenProject { get; }

	/// <summary>Gets a command for opening the selected project license.</summary>
	public CommandBase OpenLicense { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var packages = await PackageInfo.ReadAsync();
		Packages = new(packages);

		DataGridView.GroupDescriptions.Add(_groupDescription);
		DataGridView.SortDescriptions.Add(_sortDescription);
	}

	private void OpenSelected()
	{
		if (TryGetUrl(out var url))
		{
			SystemBrowser.OpenBrowser(url);
		}
	}

	private void OpenSelectedLicense()
	{
		if (TryGetLicenseUrl(out var url))
		{
			SystemBrowser.OpenBrowser(url);
		}
	}

	private bool TryGetUrl([MaybeNullWhen(false)] out string url)
	{
		url = Selected?.PackageProjectUrl?.OriginalString;
		return !string.IsNullOrWhiteSpace(url);
	}

	private bool TryGetLicenseUrl([MaybeNullWhen(false)] out string url)
	{
		url = null;
		if (Selected is null)
		{
			return false;
		}

		if (Uri.IsWellFormedUriString(Selected.License, UriKind.Absolute))
		{
			url = Selected.License;
			return true;
		}

		url = $"https://spdx.org/licenses/{Selected.License}.html";
		return true;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			OpenProject.InvokeExecuteChanged();
			OpenLicense.InvokeExecuteChanged();
		}
	}
}
