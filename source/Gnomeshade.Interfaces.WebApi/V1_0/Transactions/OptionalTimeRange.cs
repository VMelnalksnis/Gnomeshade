// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

/// <summary>
/// A date range with optional start and end dates.
/// </summary>
[PublicAPI]
[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
public sealed record OptionalTimeRange : IValidatableObject
{
	private const string _errorMessage = "The 'to' date must be before the 'from' date";

	/// <summary>
	/// The start of the date range.
	/// </summary>
	public Instant? From { get; init; }

	/// <summary>
	/// The end of the date range.
	/// </summary>
	public Instant? To { get; init; }

	/// <inheritdoc />
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var results = new List<ValidationResult>(1);
		if (From is null || To is null || To >= From)
		{
			return results;
		}

		results.Add(new(_errorMessage, new[] { nameof(From), nameof(To) }));
		return results;
	}
}
