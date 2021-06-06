// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.OpenApi
{
	public static class SwaggerGenOptionsExtensions
	{
		/// <summary>
		/// Define the document to be created by Swagger generator for version 1.0.
		/// </summary>
		///
		/// <param name="options">The <see cref="SwaggerGenOptions"/> in which to define the document.</param>
		public static void SwaggerDocV1_0(this SwaggerGenOptions options)
		{
			options
				.SwaggerDoc(
					"v1.0",
					new OpenApiInfo
					{
						Title = "Finance Tracker API",
						Version = "v1.0",
						Description = "Personal finance tracking API",
						Contact = new OpenApiContact
						{
							Name = "Valters Melnalksnis",
							Email = "valters.melnalksnis@outlook.com",
						},
						License = new OpenApiLicense
						{
							Name = "AGPL-3.0-or-later",
							Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.txt"),
						},
					});
		}
	}
}
