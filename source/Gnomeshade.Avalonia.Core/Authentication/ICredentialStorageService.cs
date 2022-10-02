// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <summary>Manages credential persistence between application runs.</summary>
public interface ICredentialStorageService
{
	/// <summary>Stores the refresh token.</summary>
	/// <param name="token">The refresh token to store.</param>
	void StoreRefreshToken(string token);

	/// <summary>Gets the stored refresh token if one is available.</summary>
	/// <param name="token">The stored refresh token if one is available; otherwise <c>null</c>.</param>
	/// <returns><c>true</c> if a stored refresh token was found; otherwise <c>false</c>.</returns>
	bool TryGetRefreshToken([NotNullWhen(true)] out string? token);

	/// <summary>Stores the users credentials.</summary>
	/// <param name="username">The username to store.</param>
	/// <param name="password">The password to store.</param>
	void StoreCredentials(string? username, string? password);

	/// <summary>Gets the stored credentials if available.</summary>
	/// <param name="username">The stored username if available; otherwise <c>null</c>.</param>
	/// <param name="password">The stored password if available; otherwise <c>null</c>.</param>
	/// <returns><c>true</c> if a stored refresh credentials were found; otherwise <c>false</c>.</returns>
	bool TryGetCredentials([NotNullWhen(true)] out string? username, [NotNullWhen(true)] out string? password);
}
