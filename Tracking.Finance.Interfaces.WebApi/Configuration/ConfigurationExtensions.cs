// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Configuration;

namespace Tracking.Finance.Interfaces.WebApi.Configuration
{
	/// <summary>
	/// Provides methods to validate objects based on DataAnnotations.
	/// </summary>
	public static class ConfigurationExtensions
	{
		public static TOptions GetValid<TOptions>(this IConfiguration configuration)
			where TOptions : notnull
		{
			var sectionName = typeof(TOptions).GetSectionName();
			return configuration.GetSection(sectionName).Get<TOptions>().ValidateAndThrow();
		}

		public static string GetSectionName(this Type type)
		{
			return type.Name.EndsWith("Options")
				? type.Name.Substring(0, type.Name.LastIndexOf("Options"))
				: type.Name;
		}

		/// <summary>
		/// Validates an object based on its DataAnnotations and throws an exception if the object is not valid.
		/// </summary>
		///
		/// <param name="options">The object to validate.</param>
		public static TOptions ValidateAndThrow<TOptions>(this TOptions options)
			where TOptions : notnull
		{
			Validator.ValidateObject(options, new ValidationContext(options), true);
			return options;
		}

		/// <summary>
		/// Validates an object based on its DataAnnotations and returns a list of validation errors.
		/// </summary>
		///
		/// <param name="options">The object to validate.</param>
		/// <returns>A list of validation errors.</returns>
		public static ICollection<ValidationResult> Validate<TOptions>(this TOptions options)
			where TOptions : notnull
		{
			var Results = new List<ValidationResult>();
			var Context = new ValidationContext(options);
			if (!Validator.TryValidateObject(options, Context, Results, true))
			{
				return Results;
			}

			return new List<ValidationResult>();
		}
	}
}
