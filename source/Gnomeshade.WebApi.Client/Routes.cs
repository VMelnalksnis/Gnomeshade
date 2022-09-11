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
	internal const string AuthenticationUri = "Authentication";
	internal const string CounterpartyUri = "Counterparties";
	internal const string CurrencyUri = "Currencies";
	internal const string Iso20022 = "Iso";
	internal const string UnitUri = "Units";
	internal const string LoginUri = $"{AuthenticationUri}/Login";
	internal const string LogOutUri = $"{AuthenticationUri}/Logout";
	internal const string SocialRegisterUri = "ExternalAuthentication/SocialRegister";

	/// <summary>Gets the relative uri for the specified counterparty.</summary>
	/// <param name="id">The id of the counterparty.</param>
	/// <returns>Relative uri for a specified counterparty.</returns>
	public static string CounterpartyIdUri(Guid id) => $"{CounterpartyUri}/{Format(id)}";

	/// <summary>Gets the relative uri for merging two counterparties.</summary>
	/// <param name="targetId">The id of the counterparty into which to merge.</param>
	/// <param name="sourceId">The id of the counterparty which to merge into the other.</param>
	/// <returns>Relative uri for merging two counterparties.</returns>
	public static string CounterpartyMergeUri(Guid targetId, Guid sourceId) =>
		$"{CounterpartyUri}/{Format(targetId)}/Merge/{Format(sourceId)}";

	/// <summary>Gets the relative uri for the specified unit.</summary>
	/// <param name="id">The id of the unit.</param>
	/// <returns>Relative uri for the specified unit.</returns>
	public static string UnitIdUri(Guid id) => $"{UnitUri}/{Format(id)}";

	private static string Format(Guid guid) => guid.ToString("N", CultureInfo.InvariantCulture);

	/// <summary>Account routes.</summary>
	public static class Accounts
	{
		internal const string Uri = nameof(Accounts);
		internal const string AllUri = $"{Uri}?onlyActive=false";

		/// <summary>Gets the relative uri for the specified account.</summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for a specific account.</returns>
		public static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string Currencies(Guid id) => $"{IdUri(id)}/{CurrencyUri}";

		internal static string CurrencyIdUri(Guid id, Guid currencyId) => $"{IdUri(id)}/{CurrencyUri}/{Format(currencyId)}";

		internal static string BalanceUri(Guid id) => $"{IdUri(id)}/Balance";
	}

	/// <summary>Transaction routes.</summary>
	public static class Transactions
	{
		internal const string Uri = "Transactions";
		private const string _detailedUri = $"{Uri}/Details";

		/// <summary>Gets the relative uri for all transactions within the specified interval.</summary>
		/// <param name="interval">The interval for which to get transactions.</param>
		/// <returns>Relative uri for all transaction with a query for the specified interval.</returns>
		public static string DateRangeUri(Interval interval) => DateRangeUri(Uri, interval);

		internal static string DetailedDateRangeUri(Interval interval) => DateRangeUri(_detailedUri, interval);

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string DetailedIdUri(Guid id) => $"{Uri}/{Format(id)}/Details";

		internal static string LinkUri(Guid id) => $"{IdUri(id)}/Links";

		internal static string LinkIdUri(Guid id, Guid linkId) => $"{LinkUri(id)}/{Format(linkId)}";

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
		internal const string Uri = "Products";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string PurchasesUri(Guid id) => $"{IdUri(id)}/Purchases";
	}

	internal static class Owners
	{
		internal const string Uri = "Owners";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Ownerships
	{
		internal const string Uri = "Ownerships";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Categories
	{
		internal const string Uri = "Categories";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Links
	{
		internal const string Uri = "Links";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Transfers
	{
		internal const string Uri = nameof(Transfers);

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{Uri}";
	}

	internal static class Purchases
	{
		internal const string Uri = nameof(Purchases);

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{Uri}";
	}

	internal static class Loans
	{
		internal const string Uri = nameof(Loans);

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/{Uri}";

		internal static string ForCounterparty(Guid counterpartyId)
		{
			const string url = $"{Transactions.Uri}/{Uri}";
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
			$"{_path}/?transactionId{Format(transactionId)}&linkId={Format(linkId)}";
	}
}
