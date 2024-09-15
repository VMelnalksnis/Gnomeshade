// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Projects;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Projects;

/// <summary>Creates or updates a single <see cref="Project"/>.</summary>
public sealed partial class ProjectUpsertionViewModel : UpsertionViewModel
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets or sets the name of the project.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the parent project of the project.</summary>
	[Notify]
	private Project? _parentProject;

	/// <summary>Gets a collection of available projects.</summary>
	[Notify(Setter.Private)]
	private List<Project> _projects = [];

	/// <summary>Gets a collection of all purchases that are a part of this project.</summary>
	[Notify(Setter.Private)]
	private List<PurchaseOverview> _purchases = [];

	/// <summary>Initializes a new instance of the <see cref="ProjectUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the project to edit.</param>
	public ProjectUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		Id = id;
	}

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Projects = await GnomeshadeClient.GetProjectsAsync();

		if (Id is not { } projectId)
		{
			return;
		}

		var (project, purchases, currencies, products, units) = await (
			GnomeshadeClient.GetProjectAsync(projectId),
			GnomeshadeClient.GetProjectPurchasesAsync(projectId),
			GnomeshadeClient.GetCurrenciesAsync(),
			GnomeshadeClient.GetProductsAsync(),
			GnomeshadeClient.GetUnitsAsync())
			.WhenAll();

		Name = project.Name;

		ParentProject = project.ParentProjectId is { } parentId
			? Projects.Single(p => p.Id == parentId)
			: null;

		Purchases = purchases
			.Select(purchase => purchase.ToOverview(currencies, products, units, Projects, _dateTimeZoneProvider))
			.ToList();
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var projectCreation = new ProjectCreation
		{
			Name = Name!,
			ParentProjectId = ParentProject?.Id,
		};

		if (Id is { } existingId)
		{
			await GnomeshadeClient.PutProjectAsync(existingId, projectCreation);
		}
		else
		{
			Id = await GnomeshadeClient.CreateProjectAsync(projectCreation);
		}

		return Id.Value;
	}
}
