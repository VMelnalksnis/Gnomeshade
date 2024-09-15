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
	internal const string AuthenticationUri = $"{V1}/Authentication";
	internal const string CounterpartyUri = $"{V1}/Counterparties";
	internal const string CurrencyUri = $"{V1}/Currencies";
	internal const string Iso20022 = $"{V1}/Iso";
	internal const string UnitUri = $"{V1}/Units";
	internal const string LoginUri = $"{AuthenticationUri}/Login";
	internal const string LogOutUri = $"{AuthenticationUri}/Logout";
	internal const string SocialRegisterUri = $"{V1}/ExternalAuthentication/SocialRegister";

	private const string V1 = "v1.0";
	private const string V2 = "v2.0";

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

	internal static string Format(Guid guid) => guid.ToString("N", CultureInfo.InvariantCulture);

	private static string Parameters(List<KeyValuePair<string, Guid>> parameters)
	{
		return string.Join("&", parameters.Select(pair => $"{pair.Key}={Format(pair.Value)}"));
	}

	/// <summary>Account routes.</summary>
	public static class Accounts
	{
		internal const string Uri = $"{V1}/Accounts";
		internal const string AllUri = $"{Uri}?onlyActive=false";

		/// <summary>Gets the relative uri for the specified account.</summary>
		/// <param name="id">The id of the account.</param>
		/// <returns>Relative uri for a specific account.</returns>
		public static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string Currencies(Guid id) => $"{IdUri(id)}/Currencies";

		internal static string CurrencyIdUri(Guid id, Guid currencyId) => $"{IdUri(id)}/Currencies/{Format(currencyId)}";

		internal static string BalanceUri(Guid id) => $"{IdUri(id)}/Balance";
	}

	/// <summary>Transaction routes.</summary>
	public static class Transactions
	{
		internal const string Uri = $"{V1}/Transactions";
		private const string _detailedUri = $"{V2}/Transactions/Details";

		/// <summary>Gets the relative uri for all transactions within the specified interval.</summary>
		/// <param name="interval">The interval for which to get transactions.</param>
		/// <returns>Relative uri for all transaction with a query for the specified interval.</returns>
		public static string DateRangeUri(Interval interval) => DateRangeUri(Uri, interval);

		internal static string DetailedDateRangeUri(Interval interval) => DateRangeUri(_detailedUri, interval);

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string DetailedIdUri(Guid id) => $"{V2}/Transactions/{Format(id)}/Details";

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
		internal const string Uri = $"{V1}/Products";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string PurchasesUri(Guid id) => $"{IdUri(id)}/Purchases";
	}

	internal static class Owners
	{
		internal const string Uri = $"{V1}/Owners";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Ownerships
	{
		internal const string Uri = $"{V1}/Ownerships";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Categories
	{
		internal const string Uri = $"{V1}/Categories";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Links
	{
		internal const string Uri = $"{V1}/Links";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class Transfers
	{
		internal const string Uri = $"{V1}/Transfers";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/Transfers";
	}

	internal static class Purchases
	{
		internal const string Uri = $"{V1}/Purchases";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string TransactionUri(Guid transactionId) => $"{Transactions.IdUri(transactionId)}/Purchases";

		internal static string ForProject(Guid projectId) => $"{Projects.IdUri(projectId)}/Purchases";

		internal static string ForProject(Guid projectId, Guid id) => $"{Projects.IdUri(projectId)}/Purchases/{Format(id)}";
	}

	internal static class Nordigen
	{
		private const string _path = $"{V1}/{nameof(Nordigen)}";

		internal static string Institutions(string country) => $"{_path}/?countryCode={country}";

		internal static string Import(string id, string timeZone) => $"{_path}/{id}/?timeZone={timeZone}";
	}

	internal static class Paperless
	{
		private const string _path = $"{V1}/{nameof(Paperless)}";

		internal static string Import(Guid transactionId, Guid linkId) =>
			$"{_path}/?{Parameters([new(nameof(transactionId), transactionId), new(nameof(linkId), linkId)])}";
	}

	internal static class Users
	{
		internal const string Uri = $"{V1}/{nameof(Users)}";
	}

	internal static class Loans
	{
		internal const string Uri = $"{V2}/{nameof(Loans)}";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}

	internal static class LoanPayments
	{
		internal const string Uri = $"{V2}/{nameof(LoanPayments)}";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";

		internal static string ForLoan(Guid loanId) =>
			$"{V2}/{nameof(Loans)}/{Format(loanId)}/{nameof(LoanPayments)}";

		internal static string ForTransaction(Guid transactionId) =>
			$"{V2}/{nameof(Transactions)}/{Format(transactionId)}/{nameof(LoanPayments)}";
	}

	internal static class Projects
	{
		internal const string Uri = $"{V1}/Projects";

		internal static string IdUri(Guid id) => $"{Uri}/{Format(id)}";
	}
}
