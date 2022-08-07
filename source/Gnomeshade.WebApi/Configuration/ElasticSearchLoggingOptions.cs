// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Configuration;

/// <summary>Options for configuring logging to ElasticSearch.</summary>
public sealed class ElasticSearchLoggingOptions
{
	/// <summary>Gets the ElasticSearch nodes to which to log to.</summary>
	[Required]
	[MinLength(1)]
	public List<Uri> Nodes { get; init; } = null!;

	/// <summary>Gets the username to use for basic authentication.</summary>
	[Required]
	public string Username { get; init; } = null!;

	/// <summary>Gets the password to use for basic authentication.</summary>
	[Required]
	public string Password { get; init; } = null!;

	/// <summary>Gets the filepath of the SSL client certificate.</summary>
	public string? CertificateFilePath { get; init; }

	/// <summary>Gets the filepath of the private key for <see cref="CertificateFilePath"/>.</summary>
	public string? KeyFilePath { get; init; }

	/// <summary>Gets the filepath of the certificate authority for validating SSL certificates.</summary>
	public string? CertificateAuthorityFilePath { get; init; }
}
