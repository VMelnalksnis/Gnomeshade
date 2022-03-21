// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Base view model for creating or updating a single entity.</summary>
public abstract class UpsertionViewModel : ViewModelBase
{
	/// <summary>Initializes a new instance of the <see cref="UpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">The strongly typed API client.</param>
	protected UpsertionViewModel(IGnomeshadeClient gnomeshadeClient)
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
	public abstract Task SaveAsync();

	/// <summary>Invokes <see cref="Upserted"/>.</summary>
	/// <param name="id">The id to pass to the <see cref="UpsertedEventArgs"/>.</param>
	protected void OnUpserted(Guid id)
	{
		Upserted?.Invoke(this, new(id));
	}
}
