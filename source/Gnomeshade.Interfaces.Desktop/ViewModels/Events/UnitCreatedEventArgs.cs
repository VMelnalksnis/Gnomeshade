// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events
{
	/// <summary>
	/// Event arguments for the <see cref="UnitCreationViewModel.UnitCreated"/> event.
	/// </summary>
	public sealed class UnitCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="unitId">The id of the unit what was created.</param>
		public UnitCreatedEventArgs(Guid unitId)
		{
			UnitId = unitId;
		}

		/// <summary>
		/// Gets the id of the unit that was created.
		/// </summary>
		public Guid UnitId { get; }
	}
}
