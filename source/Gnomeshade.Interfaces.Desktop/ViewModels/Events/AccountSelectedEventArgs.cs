// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events;

/// <summary>
/// Event arguments for <see cref="AccountViewModel.AccountSelected"/> event.
/// </summary>
public sealed class AccountSelectedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountSelectedEventArgs"/> class.
	/// </summary>
	/// <param name="accountId">The id of the selected account.</param>
	public AccountSelectedEventArgs(Guid accountId)
	{
		AccountId = accountId;
	}

	/// <summary>
	/// Gets the id of the selected account.
	/// </summary>
	public Guid AccountId { get; }
}
