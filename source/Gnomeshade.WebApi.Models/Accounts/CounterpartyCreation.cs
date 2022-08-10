﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>The information needed to create a new counterparty.</summary>
[PublicAPI]
public sealed record CounterpartyCreation : Creation
{
	/// <inheritdoc cref="Counterparty.Name"/>
	[Required]
	public string? Name { get; init; }
}