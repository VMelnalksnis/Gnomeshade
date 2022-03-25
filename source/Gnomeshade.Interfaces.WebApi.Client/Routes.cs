// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Relative URIs of API endpoints.</summary>
public static class Routes
{
	internal const string _authenticationUri = "Authentication";
	internal const string _accountUri = "Accounts";
	internal const string _counterpartyUri = "Counterparties";
	internal const string _currencyUri = "Currencies";
	internal const string _iso20022 = "Iso";
	internal const string _productUri = "Products";
	internal const string _transactionUri = "Transactions";
	internal const string _unitUri = "Units";
	internal const string _tagUri = "Tags";
	internal const string _allAccountUri = $"{_accountUri}?onlyActive=false";
	internal const string _loginUri = $"{_authenticationUri}/Login";
	internal const string _logOutUri = $"{_authenticationUri}/Logout";
	internal const string _socialRegisterUri = $"{_authenticationUri}/SocialRegister";

	/// <summary>Gets the relative uri for the specified account.</summary>
	/// <param name="id">The id of the account.</param>
	/// <returns>Relative uri for a specific account.</returns>
	public static string AccountIdUri(Guid id) => $"{_accountUri}/{Format(id)}";

	/// <summary>Gets the relative uri for the currencies of the specified account.</summary>
	/// <param name="id">The id of the account.</param>
	/// <returns>Relative uri for the currencies of the specified account.</returns>
	public static string AccountCurrencyUri(Guid id) => $"{_accountUri}/{Format(id)}/Currencies";

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

	/// <summary>Gets the relative uri for the specified transaction.</summary>
	/// <param name="id">The id of the transaction.</param>
	/// <returns>Relative uri for a specific transaction.</returns>
	public static string TransactionIdUri(Guid id) => $"{_transactionUri}/{Format(id)}";

	/// <summary>Gets the relative uri for all transactions within the specified period.</summary>
	/// <param name="from">The point in time from which to select transactions.</param>
	/// <param name="to">The point in time to which to select transactions.</param>
	/// <returns>Relative uri for all transaction with a query for the specified period.</returns>
	public static string TransactionDateRangeUri(DateTimeOffset? from, DateTimeOffset? to)
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
			return _transactionUri;
		}

		var parameters = keyValues.Select(pair => $"{pair.Value}={UrlEncodeDateTimeOffset(pair.Key)}");
		var query = string.Join('&', parameters);
		return $"{_transactionUri}?{query}";
	}

	/// <summary>Gets the relative uri for the items of the specified transaction.</summary>
	/// <param name="id">The id of the transaction.</param>
	/// <returns>Relative uri for items of a transaction.</returns>
	public static string TransactionItemUri(Guid id) => $"{TransactionIdUri(id)}/Item";

	/// <summary>Gets the relative uri for the specified transaction item.</summary>
	/// <param name="id">The id of the transaction item.</param>
	/// <returns>Relative uri for a specific transaction item.</returns>
	public static string TransactionItemIdUri(Guid id) => $"{_transactionUri}/Item/{Format(id)}";

	/// <summary>Gets the relative uri for the specified transaction and item.</summary>
	/// <param name="transactionId">The id of the transaction.</param>
	/// <param name="id">The id of the transaction item.</param>
	/// <returns>Relative uri for the specified transaction and item.</returns>
	public static string TransactionItemIdUri(Guid transactionId, Guid id) =>
		$"{TransactionIdUri(transactionId)}/Item/{Format(id)}";

	/// <summary>Gets the relative uri for the tags of the specified transaction item.</summary>
	/// <param name="id">The id of the transaction item.</param>
	/// <returns>Relative uri for the tags of the specified transaction item.</returns>
	public static string TransactionItemTagUri(Guid id) =>
		$"{TransactionItemIdUri(id)}/Tag";

	/// <summary>Gets the relative uri for the specified transaction item tag.</summary>
	/// <param name="id">The id of the transaction item.</param>
	/// <param name="tagId">The id of the tag.</param>
	/// <returns>Relative uri for the specified transaction item and tag.</returns>
	public static string TransactionItemTagIdUri(Guid id, Guid tagId) =>
		$"{TransactionItemTagUri(id)}/{Format(tagId)}";

	/// <summary>Gets the relative uri for the specified tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <returns>Relative uri for the specified tag.</returns>
	public static string TagIdUri(Guid id) => $"{_tagUri}/{Format(id)}";

	/// <summary>Gets the relative uri for the tags of the specified tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <returns>Relative uri for the tags of the specified tag.</returns>
	public static string TagTagUri(Guid id) => $"{TagIdUri(id)}/Tag";

	/// <summary>Gets the relative uri for the specified tag tag.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <param name="tagId">The id of the tag tag.</param>
	/// <returns>Relative uri for the specified tag tag.</returns>
	public static string TagTagIdUri(Guid id, Guid tagId) => $"{TagTagUri(id)}/{Format(tagId)}";

	/// <summary>Gets the relative uri for the specified unit.</summary>
	/// <param name="id">The id of the unit.</param>
	/// <returns>Relative uri for the specified unit.</returns>
	public static string UnitIdUri(Guid id) => $"{_unitUri}/{Format(id)}";

	/// <summary>Converts the specified date to a string and encodes it for using within a url.</summary>
	/// <param name="date">The date to convert.</param>
	/// <returns>A string representation of the <paramref name="date"/> that can be used in urls.</returns>
	public static string UrlEncodeDateTimeOffset(DateTimeOffset date)
	{
		var value = date.ToString("O", CultureInfo.InvariantCulture);
		return date.Offset < TimeSpan.Zero
			? value
			: value.Replace("+", "%2B");
	}

	private static string Format(Guid guid) => guid.ToString("N", CultureInfo.InvariantCulture);
}
