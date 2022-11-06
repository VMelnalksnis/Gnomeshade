// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NodaTime;

namespace Gnomeshade.WebApi.Client;

/// <summary>Relative URIs of API endpoints.</summary>
public static class Routes
{
	internal const string _authenticationUri = "Authentication";
	internal const string _counterpartyUri = "Counterparties";
	internal const string _currencyUri = "Currencies";
	internal const string _iso20022 = "Iso";
	internal const string _unitUri = "Units";
	internal const string _loginUri = $"{_authenticationUri}/Login";
	internal const string _logOutUri = $"{_authenticationUri}/Logout";
	internal const string _socialRegisterUri = "ExternalAuthentication/SocialRegister";

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

	/// <summary>Gets the relative uri for the specified unit.</summary>
	/// <param name="id">The id of the unit.</param>
	/// <returns>Relative uri for the specified unit.</returns>
	public static string UnitIdUri(Guid id) => $"{_unitUri}/{Format(id)}";

	private static string Format(Guid guid) => guid.ToString("N", CultureInfo.InvariantCulture);

	private static string Parameters(List<KeyValuePair<string, Guid>> parameters)
	{
		return string.Join("&", parameters.Select(pair => $"{pair.Key}={Format(pair.Value)}"));
	}

	/// <summary>Account routes.</summary>
	public static class Accounts
	{
		internal const string _uri = nameof(Accounts);
		internal const string _allUri = $"{_uri}?onlyActive=false";

		/// <summary>Gets the relative uri for the specified account.</summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for a specific account.</returns>
		public static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string Currencies(Guid id) => $"{IdUri(id)}/{_currencyUri}";

		internal static string CurrencyIdUri(Guid id, Guid currencyId) => $"{IdUri(id)}/{_currencyUri}/{Format(currencyId)}";

		internal static string BalanceUri(Guid id) => $"{IdUri(id)}/Balance";
	}

	/// <summary>Transaction routes.</summary>
	public static class Transactions
	{
		internal const string _uri = "Transactions";
		private const string _detailedUri = $"{_uri}/Details";

		/// <summary>Gets the relative uri for all transactions within the specified interval.</summary>
		/// <param name="interval">The interval for which to get transactions.</param>
		/// <returns>Relative uri for all transaction with a query for the specified interval.</returns>
		public static string DateRangeUri(Interval interval) => DateRangeUri(_uri, interval);

		internal static string DetailedDateRangeUri(Interval interval) => DateRangeUri(_detailedUri, interval);

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string DetailedIdUri(Guid id) => $"{_uri}/{Format(id)}/Details";

		internal static string LinkUri(Guid id) => $"{IdUri(id)}/Links";

		internal static string LinkIdUri(Guid id, Guid linkId) => $"{LinkUri(id)}/{Format(linkId)}";

		internal static string MergeUri(Guid targetId, Guid sourceId) => $"{IdUri(targetId)}/Merge/{Format(sourceId)}";

		internal static string RelatedUri(Guid id) => $"{IdUri(id)}/Related";

		internal static string RelatedUri(Guid id, Guid relatedId) => $"{RelatedUri(id)}/{Format(relatedId)}";

		private static string DateRangeUri(string baseUri, Interval interval)
		{
			var keyValues = new List<KeyValuePair<Instant, string>>(2);
			if (interval.HasStart)
			{
				keyValues.Add(new(interval.Start, "from"));
			}

			if (interval.HasEnd)
			{
				keyValues.Add(new(interval.End, "to"));
			}

			if (!keyValues.Any())
			{
				return baseUri;
			}

			var parameters = keyValues.Select(pair => $"{pair.Value}={pair.Key}");
			var query = string.Join('&', parameters);
			return $"{baseUri}?{query}";
		}
	}

	internal static class Products
	{
		internal const string _uri = "Products";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string PurchasesUri(Guid id) => $"{IdUri(id)}/Purchases";
	}

	internal static class Owners
	{
		internal const string _uri = "Owners";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";
	}

	internal static class Ownerships
	{
		internal const string _uri = "Ownerships";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";
	}

	internal static class Categories
	{
		internal const string _uri = "Categories";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";
	}

	internal static class Links
	{
		internal const string _uri = "Links";

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";
	}

	internal static class Transfers
	{
		internal const string _uri = nameof(Transfers);

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{_uri}";
	}

	internal static class Purchases
	{
		internal const string _uri = nameof(Purchases);

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{_uri}";
	}

	internal static class Loans
	{
		internal const string _uri = nameof(Loans);

		internal static string IdUri(Guid id) => $"{_uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{_uri}";

		internal static string ForCounterparty(Guid counterpartyId)
		{
			const string url = $"{Transactions._uri}/{_uri}";
			return $"{url}?counterpartyId={Format(counterpartyId)}";
		}
	}

	internal static class Nordigen
	{
		private const string _path = nameof(Nordigen);

		internal static string Institutions(string country) => $"{_path}/?countryCode={country}";

		internal static string Import(string id, string timeZone) => $"{_path}/{id}/?timeZone={timeZone}";
	}

	internal static class Paperless
	{
		private const string _path = nameof(Paperless);

		internal static string Import(Guid transactionId, Guid linkId) =>
			$"{_path}/?{Parameters(new() { new(nameof(transactionId), transactionId), new(nameof(linkId), linkId) })}";
	}
}
