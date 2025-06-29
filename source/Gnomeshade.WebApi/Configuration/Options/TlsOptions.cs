﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options for configuring TLS settings not exposed by Kestrel configuration.</summary>
public sealed record TlsOptions : IValidatableObject
{
	/// <summary>Gets allowed cipher suites for <see cref="SslServerAuthenticationOptions"/>.</summary>
	/// <seealso href="https://github.com/dotnet/runtime/issues/23818#issuecomment-482764511"/>
	[Required]
	[MinLength(1)]
	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"Implements {nameof(ICollection)}")]
	public List<TlsCipherSuite> CipherSuites { get; init; } = null!;

	/// <inheritdoc />
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var validationResults = new List<ValidationResult>();

		if (OperatingSystem.IsWindows())
		{
			validationResults.Add(new("Configuring cipher suites is not supported on windows", [nameof(CipherSuites)]));
		}

		return validationResults;
	}
}
