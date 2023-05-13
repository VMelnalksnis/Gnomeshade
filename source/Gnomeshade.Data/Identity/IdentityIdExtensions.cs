// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

namespace Gnomeshade.Data.Identity;

/// <summary>Methods for converting <see cref="Guid"/> to and from <see cref="string"/> for identity managers.</summary>
public static class IdentityIdExtensions
{
	private const string _guidFormat = "D";

	/// <summary>Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure.</summary>
	/// <param name="id">The string id to convert.</param>
	/// <returns>An instance of <see cref="Guid"/> representing the provided <paramref name="id"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
	public static Guid ConvertIdFromString(this string? id)
	{
		ArgumentNullException.ThrowIfNull(id, nameof(id));
		return Guid.ParseExact(id, _guidFormat);
	}

	/// <summary>Converts the provided <paramref name="id"/> to its string representation.</summary>
	/// <param name="id">The id to convert to string.</param>
	/// <returns>The string representation of <paramref name="id"/>.</returns>
	public static string ConvertIdToString(this Guid id) => id.ToString(_guidFormat, CultureInfo.InvariantCulture);
}
