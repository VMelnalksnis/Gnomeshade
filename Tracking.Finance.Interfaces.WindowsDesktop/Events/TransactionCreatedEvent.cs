// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Interfaces.WindowsDesktop.Events
{
	/// <summary>
	/// Indicates that a new transaction has been created.
	/// </summary>
	///
	/// <param name="Id">The id of the created transaction.</param>
	public sealed record TransactionCreatedEvent(int Id);
}
