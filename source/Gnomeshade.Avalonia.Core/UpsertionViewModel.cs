// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia.Controls.Notifications;

using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Base view model for creating or updating a single entity.</summary>
public abstract partial class UpsertionViewModel : ViewModelBase
{
	/// <summary>Gets or sets the id of the entity being edited.</summary>
	[Notify(Setter.Protected)]
	private Guid? _id;

	/// <summary>Initializes a new instance of the <see cref="UpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">The strongly typed API client.</param>
	protected UpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		GnomeshadeClient = gnomeshadeClient;
	}

	/// <summary>Raised when a new entity has been successfully created or an existing one has been updated.</summary>
	public event EventHandler<UpsertedEventArgs>? Upserted;

	/// <summary>Gets a value indicating whether the current state of the view model represents a valid entity.</summary>
	public abstract bool CanSave { get; }

	/// <summary>Gets the strongly typed API client.</summary>
	protected IGnomeshadeClient GnomeshadeClient { get; }

	/// <summary>Saves the current state of the view model.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SaveAsync()
	{
		if (!CanSave)
		{
			ActivityService.ShowNotification(new("Cannot save", "Please check whether all data is valid before saving", NotificationType.Warning));
			return;
		}

		using var activity = BeginActivity("Saving");
		try
		{
			var id = await SaveValidatedAsync();
			OnUpserted(id);
			Id = id;
		}
		catch (Exception exception)
		{
			ActivityService.ShowErrorNotification(exception);
		}
	}

	/// <summary>Saves the current state of the view model.</summary>
	/// <returns>The id of the saved entity.</returns>
	protected abstract Task<Guid> SaveValidatedAsync();

	/// <summary>Invokes <see cref="Upserted"/>.</summary>
	/// <param name="id">The id to pass to the <see cref="UpsertedEventArgs"/>.</param>
	protected void OnUpserted(Guid id)
	{
		Upserted?.Invoke(this, new(id));
	}
}
