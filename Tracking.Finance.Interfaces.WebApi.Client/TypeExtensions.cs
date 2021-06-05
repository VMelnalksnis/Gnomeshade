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
