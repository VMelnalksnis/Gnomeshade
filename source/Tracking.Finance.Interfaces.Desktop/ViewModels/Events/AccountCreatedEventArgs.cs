﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels.Events
{
	/// <summary>
	/// Event arguments for <see cref="AccountCreationViewModel.AccountCreated"/> event.
	/// </summary>
	public sealed class AccountCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="accountId">The id of the created account.</param>
		public AccountCreatedEventArgs(Guid accountId)
		{
			AccountId = accountId;
		}

		/// <summary>
		/// Gets the id of the created account.
		/// </summary>
		public Guid AccountId { get; }
	}
}
