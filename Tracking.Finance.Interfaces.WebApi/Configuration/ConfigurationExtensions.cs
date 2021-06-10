// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;

namespace Tracking.Finance.Interfaces.WebApi.Configuration
{
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

		/// <summary>
		/// Validates an object based on its DataAnnotations and returns a list of validation errors.
		/// </summary>
		///
		/// <typeparam name="TOptions">The type of the options object.</typeparam>
		/// <param name="options">The object to validate.</param>
		/// <returns>A list of validation errors.</returns>
		public static ICollection<ValidationResult> Validate<TOptions>(this TOptions options)
			where TOptions : notnull
		{
			var results = new List<ValidationResult>();
			var context = new ValidationContext(options);
			return !Validator.TryValidateObject(options, context, results, true) ? results : new();
		}

		private static TOptions ValidateAndThrow<TOptions>(this TOptions options)
			where TOptions : notnull
		{
			Validator.ValidateObject(options, new(options), true);
			return options;
		}
	}
}
