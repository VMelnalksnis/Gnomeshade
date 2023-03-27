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
using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gnomeshade.Desktop.Authentication;

/// <summary>Manages credential persistence on windows.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsCredentialStorageService : ICredentialStorageService
{
	private const string _tokenAttributeName = "Token";

	private readonly ILogger<WindowsCredentialStorageService> _logger;
	private readonly IOptionsMonitor<GnomeshadeOptions> _optionsMonitor;

	/// <summary>Initializes a new instance of the <see cref="WindowsCredentialStorageService"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="optionsMonitor">Gnomeshade client options monitor.</param>
	public WindowsCredentialStorageService(
		ILogger<WindowsCredentialStorageService> logger,
		IOptionsMonitor<GnomeshadeOptions> optionsMonitor)
	{
		_logger = logger;
		_optionsMonitor = optionsMonitor;
	}

	/// <inheritdoc />
	public void StoreRefreshToken(string token)
	{
		var credentials = new NetworkCredential().ToICredential();
		credentials.TargetName = GetCredentialName();
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

		if (credential.Attributes is null ||
			!credential.Attributes.TryGetValue(_tokenAttributeName, out var value) ||
			value is not bool flag ||
			!flag)
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
		CredentialManager.SaveCredentials(GetCredentialName(), credentials);
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

		if (credential.Attributes?.Any() ?? false)
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
		var credentialName = GetCredentialName();

		try
		{
			CredentialManager.RemoveCredentials(credentialName);
		}
		catch (CredentialAPIException exception)
		{
			_logger.LogWarning(exception, "Failed to remove credentials");
		}
	}

	private bool TryGetCredential([NotNullWhen(true)] out ICredential? credential)
	{
		var credentialName = GetCredentialName();
		credential = CredentialManager.GetICredential(credentialName);
		return credential is not null;
	}

	private string GetCredentialName()
	{
		const string credentialName = "Gnomeshade";
		var serverHost = _optionsMonitor.CurrentValue.BaseAddress?.Host;

		return string.IsNullOrWhiteSpace(serverHost)
			? credentialName
			: $"{credentialName}:{serverHost}";
	}
}
