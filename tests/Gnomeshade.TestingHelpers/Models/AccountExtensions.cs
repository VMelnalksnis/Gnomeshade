// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;

namespace Gnomeshade.TestingHelpers.Models;

public static class AccountExtensions
{
	public static AccountCreation ToCreation(this Account account) => new()
	{
		OwnerId = account.OwnerId,
		Name = account.Name,
		CounterpartyId = account.CounterpartyId,
		PreferredCurrencyId = account.PreferredCurrency.Id,
		Currencies = account.Currencies.Select(currency => new AccountInCurrencyCreation
		{
			CurrencyId = currency.CurrencyId,
		}).ToList(),
		AccountNumber = account.AccountNumber,
		Iban = account.Iban,
		Bic = account.Bic,
	};
}
