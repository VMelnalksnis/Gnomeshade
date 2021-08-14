// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Authentication
{
	/// <summary>
	/// Information about the started session.
	/// </summary>
	[PublicAPI]
	public sealed record LoginResponse(string Token, DateTime ValidTo)
	{
		/// <summary>
		/// A JWT for authenticating the session.
		/// </summary>
		public string Token { get; init; } = Token;

		/// <summary>
		/// The point in time until which the session is valid.
		/// </summary>
		public DateTime ValidTo { get; init; } = ValidTo;
	}
}
