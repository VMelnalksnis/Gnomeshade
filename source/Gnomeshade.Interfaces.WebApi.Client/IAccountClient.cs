// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the account API.
	/// </summary>
	public interface IAccountClient
	{
		/// <summary>
		/// Gets the counterparty that represents the current user.
		/// </summary>
		/// <returns>The counterparty that represents the current user.</returns>
		Task<Counterparty> GetMyCounterpartyAsync();

		/// <summary>
		/// Finds an account with the specified id.
		/// </summary>
		/// <param name="id">The id by which to search for an account.</param>
		/// <returns>The account with the specified id if it exists; otherwise <see langword="null"/>.</returns>
		Task<Account> GetAccountAsync(Guid id);

		/// <summary>
		/// Finds an account with the specified normalized name.
		/// </summary>
		/// <param name="name">The normalized by which to search for an account.</param>
		/// <returns>The account with the specified name if it exists; otherwise <see langword="null"/>.</returns>
		Task<Account?> FindAccountAsync(string name);

		/// <summary>
		/// Gets all accounts.
		/// </summary>
		/// <returns>A collection with all accounts.</returns>
		Task<List<Account>> GetAccountsAsync();

		/// <summary>
		/// Gets all currently active accounts.
		/// </summary>
		/// <returns>A collection with all currently active accounts.</returns>
		Task<List<Account>> GetActiveAccountsAsync();

		/// <summary>
		/// Creates a new account.
		/// </summary>
		/// <param name="account">Information for creating the account.</param>
		/// <returns>The id of the created account.</returns>
		Task<Guid> CreateAccountAsync(AccountCreationModel account);

		/// <summary>
		/// Adds a currency to an existing account.
		/// </summary>
		/// <param name="id">The id of the account to which to add the currency.</param>
		/// <param name="currency">The currency which to add to the account.</param>
		/// <returns>The id of the account to which the currency was added to.</returns>
		Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency);

		/// <summary>
		/// Gets all currencies.
		/// </summary>
		/// <returns>A collection with all currencies.</returns>
		Task<List<Currency>> GetCurrenciesAsync();
	}
}
