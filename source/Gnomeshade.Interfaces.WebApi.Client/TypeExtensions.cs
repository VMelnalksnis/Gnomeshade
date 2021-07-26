// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	public static class TypeExtensions
	{
		public static string GetControllerName(this Type controllerType)
		{
			var isController =
				controllerType.IsAssignableTo(typeof(ControllerBase)) ||
				controllerType.GetCustomAttributes(true).OfType<ApiControllerAttribute>().Any();

			if (!isController)
			{
				throw new ArgumentException("The specified type must be a controller", nameof(controllerType));
			}

			return controllerType.Name[..controllerType.Name.LastIndexOf("Controller", StringComparison.Ordinal)];
		}
	}
}
