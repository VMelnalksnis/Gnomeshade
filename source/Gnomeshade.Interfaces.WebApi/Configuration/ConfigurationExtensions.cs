// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

/// <summary>
/// Provides methods to validate objects based on DataAnnotations.
/// </summary>
public static class ConfigurationExtensions
{
	public static TOptions GetValid<[MeansImplicitUse(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]TOptions>(this IConfiguration configuration)
		where TOptions : notnull
	{
		var sectionName = typeof(TOptions).GetSectionName();
		return configuration.GetSection(sectionName).Get<TOptions>().ValidateAndThrow();
	}

	public static string GetSectionName(this Type type)
	{
		return type.Name.EndsWith("Options")
			? type.Name[..type.Name.LastIndexOf("Options", StringComparison.Ordinal)]
			: type.Name;
	}

	private static TOptions ValidateAndThrow<TOptions>(this TOptions options)
		where TOptions : notnull
	{
		Validator.ValidateObject(options, new(options), true);
		return options;
	}
}
