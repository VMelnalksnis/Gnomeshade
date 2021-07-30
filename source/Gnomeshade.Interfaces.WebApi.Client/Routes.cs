﻿// Copyright 2021 Valters Melnalksnis
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

		/// <summary>
		/// Gets the relative uri for the specified account.
		/// </summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for a specific account.</returns>
		public static string AccountUri(Guid id) => $"{Account}/{id:N}";

		/// <summary>
		/// Gets the relative uri for the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction.</param>
		/// <returns>Relative uri for a specific transaction.</returns>
		public static string TransactionUri(Guid id) => $"{Transaction}/{id:N}";

		/// <summary>
		/// Gets the relative uri for all transactions within the specified period.
		/// </summary>
		/// <param name="from">The point in time from which to select transactions.</param>
		/// <param name="to">The point in time to which to select transactions.</param>
		/// <returns>Relative uri for all transaction with a query for the specified period.</returns>
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

		/// <summary>
		/// Gets the relative uri for the specified transaction item.
		/// </summary>
		/// <param name="id">The id of the transaction item.</param>
		/// <returns>Relative uri for a specific transaction item.</returns>
		public static string TransactionItemUri(Guid id) => $"{Transaction}/item/{id:N}";

		/// <summary>
		/// Converts the specified date to a string and encodes it for using within a url.
		/// </summary>
		/// <param name="date">The date to convert.</param>
		/// <returns>A string representation of the <paramref name="date"/> that can be used in urls.</returns>
		public static string UrlEncodeDateTimeOffset(DateTimeOffset date)
		{
			var value = date.ToString("O");
			return date.Offset <= TimeSpan.Zero
				? value
				: value.Replace("+", "%2B");
		}
	}
}
