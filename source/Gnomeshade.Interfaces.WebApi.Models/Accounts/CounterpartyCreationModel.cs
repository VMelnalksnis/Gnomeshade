// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Accounts;

/// <summary>
/// The information needed to create a new counterparty.
/// </summary>
[PublicAPI]
public sealed record CounterpartyCreationModel
{
	/// <summary>
	/// The name of the counterparty to create. Required.
	/// </summary>
	[Required]
	public string? Name { get; init; }
}
