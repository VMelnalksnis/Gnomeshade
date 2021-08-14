// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Accounts
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record CounterpartyCreationModel
	{
		/// <summary>
		/// The name of the counterparty to create. Required.
		/// </summary>
		[Required]
		public string? Name { get; init; }
	}
}
