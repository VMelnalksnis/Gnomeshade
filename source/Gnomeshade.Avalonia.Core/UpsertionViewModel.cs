// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Base view model for creating or updating a single entity.</summary>
public abstract class UpsertionViewModel : ViewModelBase
{
	/// <summary>Params for <see cref="PropertyChangedBase.SetAndNotifyWithGuard{T}"/> for properties that need to notify <see cref="CanSave"/>.</summary>
	protected static readonly string[] CanSaveNames = { nameof(CanSave) };

	private string? _errorMessage;

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

	/// <summary>Gets or sets error message when <see cref="SaveAsync"/> fails.</summary>
	public string? ErrorMessage
	{
		get => _errorMessage;
		protected set => SetAndNotify(ref _errorMessage, value);
	}

	/// <summary>Gets the strongly typed API client.</summary>
	protected IGnomeshadeClient GnomeshadeClient { get; }

	/// <summary>Saves the current state of the view model.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SaveAsync()
	{
		if (!CanSave)
		{
			ErrorMessage = $"{nameof(CanSave)} must be true when calling {nameof(SaveAsync)}";
			return;
		}

		try
		{
			var id = await SaveValidatedAsync();
			ErrorMessage = null;
			OnUpserted(id);
		}
		catch (Exception exception)
		{
			Debug.WriteLine(exception.ToString());
			ErrorMessage = exception.Message;
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
