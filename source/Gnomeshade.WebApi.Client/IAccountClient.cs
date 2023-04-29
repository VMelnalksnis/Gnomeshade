// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models.Accounts;

namespace Gnomeshade.WebApi.Client;

/// <summary>Provides typed interface for using the account API.</summary>
public interface IAccountClient
{
	/// <summary>Gets the counterparty that represents the current user.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The counterparty that represents the current user.</returns>
	Task<Counterparty> GetMyCounterpartyAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the counterparty with the specified id.</summary>
	/// <param name="id">The id by which to search for the counterparty.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The counterparty with the specified id if it exists.</returns>
	Task<Counterparty> GetCounterpartyAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all counterparties.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection of all counterparties.</returns>
	Task<List<Counterparty>> GetCounterpartiesAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new counterparty.</summary>
	/// <param name="counterparty">The counterparty to create.</param>
	/// <returns>The id of the created counterparty.</returns>
	Task<Guid> CreateCounterpartyAsync(CounterpartyCreation counterparty);

	/// <summary>Creates a new counterparty, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the counterparty.</param>
	/// <param name="counterparty">The counterparty to create or update.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutCounterpartyAsync(Guid id, CounterpartyCreation counterparty);

	/// <summary>Merges one counterparty into another.</summary>
	/// <param name="targetId">The id of the counterparty in to which to merge.</param>
	/// <param name="sourceId">The id of the counterparty which to merge into the other one.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task MergeCounterpartiesAsync(Guid targetId, Guid sourceId);

	/// <summary>Finds an account with the specified id.</summary>
	/// <param name="id">The id by which to search for an account.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The account with the specified id.</returns>
	Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all accounts.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all accounts.</returns>
	Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new account.</summary>
	/// <param name="account">Information for creating the account.</param>
	/// <returns>The id of the created account.</returns>
	Task<Guid> CreateAccountAsync(AccountCreation account);

	/// <summary>Creates a new account or replaces an existing  one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the account.</param>
	/// <param name="account">The account to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutAccountAsync(Guid id, AccountCreation account);

	/// <summary>Adds a currency to an existing account.</summary>
	/// <param name="id">The id of the account to which to add the currency.</param>
	/// <param name="currency">The currency which to add to the account.</param>
	/// <returns>The id of the account to which the currency was added to.</returns>
	Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreation currency);

	/// <summary>Removes a currency from an existing account.</summary>
	/// <param name="id">The id of the account from which to remove the currency.</param>
	/// <param name="currencyId">The id of the account in currency which to remove.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task RemoveCurrencyFromAccountAsync(Guid id, Guid currencyId);

	/// <summary>Gets all currencies.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all currencies.</returns>
	Task<List<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the current balance of the specified account.</summary>
	/// <param name="id">The id of the account for which to get the balance for.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The balance of the specified account.</returns>
	Task<List<Balance>> GetAccountBalanceAsync(Guid id, CancellationToken cancellationToken = default);
}
