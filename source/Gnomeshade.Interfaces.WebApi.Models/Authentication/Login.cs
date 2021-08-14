// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Authentication
{
	/// <summary>
	/// The information needed to log in.
	/// </summary>
	[PublicAPI]
	public sealed record Login
	{
		/// <summary>
		/// The username to log in with. Required.
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string? Username { get; init; }

		/// <summary>
		/// The password to log in with. Required.
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string? Password { get; init; }
	}
}
