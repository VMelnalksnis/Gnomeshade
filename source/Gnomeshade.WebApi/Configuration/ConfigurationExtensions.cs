﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;

using static JetBrains.Annotations.ImplicitUseKindFlags;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Gnomeshade.WebApi.Configuration;

internal static class ConfigurationExtensions
{
	internal static bool GetValidIfDefined<[MeansImplicitUse(Assign, Members)] TOptions>(
		this IConfiguration configuration,
		[MaybeNullWhen(false)] out TOptions options)
		where TOptions : notnull
	{
		if (!configuration.IsSectionDefined<TOptions>())
		{
			options = default;
			return false;
		}

		options = configuration.GetValid<TOptions>();
		return true;
	}

	internal static TOptions GetValid<[MeansImplicitUse(Assign, Members)] TOptions>(this IConfiguration configuration)
		where TOptions : notnull
	{
		var sectionName = typeof(TOptions).GetSectionName();
		return configuration.GetSection(sectionName).Get<TOptions>().ValidateAndThrow();
	}

	internal static string GetSectionName(this Type type)
	{
		return type.Name.EndsWith("Options")
			? type.Name[..type.Name.LastIndexOf("Options", StringComparison.Ordinal)]
			: type.Name;
	}

	private static bool IsSectionDefined<TOptions>(this IConfiguration configuration)
	{
		var sectionName = typeof(TOptions).GetSectionName();
		return configuration.GetChildren().Any(section => section.Key == sectionName);
	}

	private static TOptions ValidateAndThrow<TOptions>(this TOptions options)
		where TOptions : notnull
	{
		Validator.ValidateObject(options, new(options), true);
		return options;
	}
}