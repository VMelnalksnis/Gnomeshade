﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

namespace Tracking.Finance.Interfaces.WebApi.Client
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

			return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
		}
	}
}
