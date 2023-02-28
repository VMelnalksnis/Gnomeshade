// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;

using AdysTech.CredentialManager;

using Gnomeshade.Avalonia.Core.Authentication;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Desktop.Authentication;

/// <summary>Manages credential persistence on windows.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsCredentialStorageService : ICredentialStorageService
{
	private const string _credentialName = "Gnomeshade";
	private const string _tokenAttributeName = "Token";

	private readonly ILogger<WindowsCredentialStorageService> _logger;

	/// <summary>Initializes a new instance of the <see cref="WindowsCredentialStorageService"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public WindowsCredentialStorageService(ILogger<WindowsCredentialStorageService> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public void StoreRefreshToken(string token)
	{
		var credentials = new NetworkCredential().ToICredential();
		credentials.TargetName = _credentialName;
		credentials.CredentialBlob = token;
		credentials.Attributes = new Dictionary<string, object> { { _tokenAttributeName, true } };
		credentials.Persistance = Persistance.LocalMachine;
		credentials.SaveCredential(true);
	}

	/// <inheritdoc />
	public bool TryGetRefreshToken([NotNullWhen(true)] out string? token)
	{
		token = null;
		if (!TryGetCredential(out var credential))
		{
			return false;
		}

		if (!credential.Attributes.TryGetValue(_tokenAttributeName, out var value) || value is not bool flag || !flag)
		{
			return false;
		}

		token = credential.CredentialBlob;
		return true;
	}

	/// <inheritdoc />
	public void StoreCredentials(string? username, string? password)
	{
		var credentials = new NetworkCredential(username, password);
		CredentialManager.SaveCredentials(_credentialName, credentials);
	}

	/// <inheritdoc />
	public bool TryGetCredentials([NotNullWhen(true)] out string? username, [NotNullWhen(true)] out string? password)
	{
		username = null;
		password = null;
		if (!TryGetCredential(out var credential))
		{
			return false;
		}

		if (credential.Attributes.Any())
		{
			return false;
		}

		username = credential.UserName;
		password = credential.CredentialBlob;
		return true;
	}

	/// <inheritdoc />
	public void RemoveCredentials()
	{
		try
		{
			CredentialManager.RemoveCredentials(_credentialName);
		}
		catch (CredentialAPIException exception)
		{
			_logger.LogWarning(exception, "Failed to remove credentials");
		}
	}

	private static bool TryGetCredential([NotNullWhen(true)] out ICredential? credential)
	{
		credential = CredentialManager.GetICredential(_credentialName);
		return credential is not null;
	}
}
