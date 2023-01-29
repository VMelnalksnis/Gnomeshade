// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

using static JetBrains.Annotations.ImplicitUseKindFlags;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Gnomeshade.WebApi.Configuration;

internal static class ConfigurationExtensions
{
	internal static bool GetValidIfDefined<[MeansImplicitUse(Assign, Members)] [DynamicallyAccessedMembers(All)] TOptions>(
		this IConfiguration configuration,
		[MaybeNullWhen(false)] out TOptions options)
		where TOptions : notnull, new()
	{
		if (!configuration.IsSectionDefined<TOptions>())
		{
			options = default;
			return false;
		}

		options = configuration.GetValid<TOptions>();
		return true;
	}

	internal static TOptions GetValid<[MeansImplicitUse(Assign, Members)] [DynamicallyAccessedMembers(All)] TOptions>(
		this IConfiguration configuration)
		where TOptions : notnull, new()
	{
		var sectionName = typeof(TOptions).GetSectionName();
		return configuration.GetValid<TOptions>(sectionName);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"{nameof(DynamicallyAccessedMembersAttribute)} indicates what is dynamically accessed")]
	internal static TOptions GetValid<[MeansImplicitUse(Assign, Members)] [DynamicallyAccessedMembers(All)] TOptions>(
		this IConfiguration configuration,
		string sectionName)
		where TOptions : notnull, new()
	{
		return (configuration.GetSection(sectionName).Get<TOptions>() ?? new()).ValidateAndThrow();
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

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"{nameof(DynamicallyAccessedMembersAttribute)} indicates what is dynamically accessed")]
	private static TOptions ValidateAndThrow<[DynamicallyAccessedMembers(PublicProperties)] TOptions>(
		this TOptions options)
		where TOptions : notnull
	{
		Validator.ValidateObject(options, new(options), true);
		return options;
	}
}
