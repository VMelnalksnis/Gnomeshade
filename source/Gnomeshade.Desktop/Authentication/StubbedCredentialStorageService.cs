// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using Gnomeshade.Avalonia.Core.Authentication;

namespace Gnomeshade.Desktop.Authentication;

internal sealed class StubbedCredentialStorageService : ICredentialStorageService
{
	/// <inheritdoc />
	public void StoreRefreshToken(string token)
	{
	}

	/// <inheritdoc />
	public bool TryGetRefreshToken([NotNullWhen(true)] out string? token)
	{
		token = null;
		return false;
	}

	/// <inheritdoc />
	public void StoreCredentials(string? username, string? password)
	{
	}

	/// <inheritdoc />
	public bool TryGetCredentials([NotNullWhen(true)] out string? username, [NotNullWhen(true)] out string? password)
	{
		username = null;
		password = null;
		return false;
	}

	/// <inheritdoc />
	public void RemoveCredentials()
	{
	}
}
