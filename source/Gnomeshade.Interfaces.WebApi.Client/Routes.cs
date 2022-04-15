﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Relative URIs of API endpoints.</summary>
public static class Routes
{
	internal const string _authenticationUri = "Authentication";
	internal const string _counterpartyUri = "Counterparties";
	internal const string _currencyUri = "Currencies";
	internal const string _iso20022 = "Iso";
	internal const string _productUri = "Products";
	internal const string _unitUri = "Units";
	internal const string _tagUri = "Tags";
	internal const string _loginUri = $"{_authenticationUri}/Login";
	internal const string _logOutUri = $"{_authenticationUri}/Logout";
	internal const string _socialRegisterUri = $"{_authenticationUri}/SocialRegister";

	/// <summary>Gets the relative uri for the specified counterparty.</summary>
	/// <param name="id">The id of the counterparty.</param>
	/// <returns>Relative uri for a specified counterparty.</returns>
	public static string CounterpartyIdUri(Guid id) => $"{_counterpartyUri}/{Format(id)}";

	/// <summary>Gets the relative uri for merging two counterparties.</summary>
	/// <param name="targetId">The id of the counterparty into which to merge.</param>
	/// <param name="sourceId">The id of the counterparty which to merge into the other.</param>
	/// <returns>Relative uri for merging two counterparties.</returns>
	public static string CounterpartyMergeUri(Guid targetId, Guid sourceId) =>
		$"{_counterpartyUri}/{Format(targetId)}/Merge/{Format(sourceId)}";

	/// <summary>Gets the relative uri for the specified product.</summary>
	/// <param name="id">The id of the product.</param>
	/// <returns>Relative uri for a specific account.</returns>
	public static string ProductIdUri(Guid id) => $"{_productUri}/{Format(id)}";

	/// <summary>Gets the relative uri for the specified tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <returns>Relative uri for the specified tag.</returns>
	public static string TagIdUri(Guid id) => $"{_tagUri}/{Format(id)}";

	/// <summary>Gets the relative uri for the tags of the specified tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <returns>Relative uri for the tags of the specified tag.</returns>
	public static string TagTagUri(Guid id) => $"{TagIdUri(id)}/{_tagUri}";

	/// <summary>Gets the relative uri for the specified tag tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <param name="tagId">The id of the tag tag.</param>
	/// <returns>Relative uri for the specified tag tag.</returns>
	public static string TagTagIdUri(Guid id, Guid tagId) => $"{TagTagUri(id)}/{Format(tagId)}";

	/// <summary>Gets the relative uri for the specified unit.</summary>
	/// <param name="id">The id of the unit.</param>
	/// <returns>Relative uri for the specified unit.</returns>
	public static string UnitIdUri(Guid id) => $"{_unitUri}/{Format(id)}";

	private static string Format(Guid guid) => guid.ToString("N", CultureInfo.InvariantCulture);

	/// <summary>Account routes.</summary>
	public static class Accounts
	{
		internal const string _uri = nameof(Accounts);
		internal const string _allUri = $"{_uri}?onlyActive=false";

		/// <summary>Gets the relative uri for the specified account.</summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for a specific account.</returns>
		public static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		/// <summary>Gets the relative uri for the currencies of the specified account.</summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for the currencies of the specified account.</returns>
		public static string CurrencyUri(Guid id) => $"{_uri}/{Format(id)}/{_currencyUri}";
	}

	/// <summary>Transaction routes.</summary>
	public static class Transactions
	{
		internal const string _uri = "Transactions";

		/// <summary>Gets the relative uri for all transactions within the specified period.</summary>
		/// <param name="from">The point in time from which to select transactions.</param>
		/// <param name="to">The point in time to which to select transactions.</param>
		/// <returns>Relative uri for all transaction with a query for the specified period.</returns>
		public static string DateRangeUri(Instant? from, Instant? to)
		{
			var keyValues = new Dictionary<Instant, string>(2);
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
				return _uri;
			}

			var parameters = keyValues.Select(pair => $"{pair.Value}={pair.Key}");
			var query = string.Join('&', parameters);
			return $"{_uri}?{query}";
		}

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string LinkUri(Guid id) => $"{IdUri(id)}/Links";

		internal static string LinkIdUri(Guid id, Guid linkId) => $"{LinkUri(id)}/{Format(linkId)}";
	}

	internal static class Links
	{
		internal const string _uri = "Links";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";
	}

	internal static class Transfers
	{
		private const string _path = nameof(Transfers);

		internal static string Uri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{_path}";

		internal static string IdUri(Guid transactionId, Guid id) => $"{Uri(transactionId)}/{Format(id)}";
	}

	internal static class Purchases
	{
		private const string _path = nameof(Purchases);

		internal static string Uri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{_path}";

		internal static string IdUri(Guid transactionId, Guid id) => $"{Uri(transactionId)}/{Format(id)}";
	}
}
