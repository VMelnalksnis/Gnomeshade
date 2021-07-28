// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Relative URIs of API endpoints.
	/// </summary>
	public static class Routes
	{
		internal static readonly string Authentication = typeof(AuthenticationController).GetControllerName();
		internal static readonly string Account = typeof(AccountController).GetControllerName();
		internal static readonly string Currency = typeof(CurrencyController).GetControllerName();
		internal static readonly string Product = typeof(ProductController).GetControllerName();
		internal static readonly string Transaction = typeof(TransactionController).GetControllerName();
		internal static readonly string Unit = typeof(UnitController).GetControllerName();

		internal static readonly string LoginUri = $"{Authentication}/{nameof(AuthenticationController.Login)}";
		internal static readonly string InfoUri = $"{Authentication}/{nameof(AuthenticationController.Info)}";

		public static string AccountUri(Guid id) => $"{Account}/{id:N}";

		public static string TransactionUri(DateTimeOffset? from, DateTimeOffset? to)
		{
			var keyValues = new Dictionary<DateTimeOffset, string>(2);
			if (from.HasValue)
			{
				keyValues.Add(from.Value, "from");
			}

			if (to.HasValue)
			{
				keyValues.Add(to.Value, "to");
			}

			if (!keyValues.Any())
			{
				return Transaction;
			}

			var parameters = keyValues.Select(pair => $"{pair.Value}={UrlEncodeDateTimeOffset(pair.Key)}");
			var query = string.Join('&', parameters);
			return $"{Transaction}?{query}";
		}

		public static string UrlEncodeDateTimeOffset(DateTimeOffset date)
		{
			var value = date.ToString("O");
			return date.Offset <= TimeSpan.Zero
				? value
				: value.Replace("+", "%2B");
		}
	}
}
