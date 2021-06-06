// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	/// <summary>
	/// Adds <see cref="StatusCodes.Status401Unauthorized"/> to all operations that require authorization.
	/// </summary>
	public sealed class UnauthorizedOperationFilter : IOperationFilter
	{
		private static readonly string UnauthorizedCode = StatusCodes.Status401Unauthorized.ToString();

		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if ((!HasAuthorizationAttribute(context.MethodInfo) &&
				!HasAuthorizationAttribute(context.MethodInfo?.DeclaringType)) ||
				operation.Responses.ContainsKey(UnauthorizedCode))
			{
				return;
			}

			operation.Responses.Add(UnauthorizedCode, new OpenApiResponse { Description = "Unauthorized" });
		}

		private bool HasAuthorizationAttribute(ICustomAttributeProvider? customAttributeProvider)
		{
			return customAttributeProvider?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false;
		}
	}
}
