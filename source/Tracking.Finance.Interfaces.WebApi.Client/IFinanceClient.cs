// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WebApi.Client.Login;
using Tracking.Finance.Interfaces.WebApi.V1_0.Accounts;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the API provided by the Interfaces.WebApi project.
	/// </summary>
	public interface IFinanceClient
	{
		/// <summary>
		/// Log in using the specified credentials.
		/// </summary>
		/// <param name="login">The credentials to use to log in.</param>
		/// <returns>Object indicating whether the login was successful or not.</returns>
		Task<LoginResult> LogInAsync(LoginModel login);

		/// <summary>
		/// Log out.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogOutAsync();

		/// <summary>
		/// Gets information about the currently logged in user.
		/// </summary>
		/// <returns>Information about the currently logged in user.</returns>
		Task<UserModel> InfoAsync();

		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		/// <param name="transaction">Information for creating the transaction.</param>
		/// <returns>The id of the created transaction.</returns>
		Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction);

		/// <summary>
		/// Gets all transactions.
		/// </summary>
		/// <returns>A collection with all transactions.</returns>
		Task<List<TransactionModel>> GetTransactionsAsync();

		/// <summary>
		/// Finds an account with the specified id.
		/// </summary>
		/// <param name="id">The id by which to search for an account.</param>
		/// <returns>The account with the specified id if it exists; otherwise <see langword="null"/>.</returns>
		Task<AccountModel> GetAccountAsync(Guid id);

		/// <summary>
		/// Gets all accounts.
		/// </summary>
		/// <returns>A collection with all accounts.</returns>
		Task<List<AccountModel>> GetAccountsAsync();

		/// <summary>
		/// Creates a new account.
		/// </summary>
		/// <param name="account">Information for creating the account.</param>
		/// <returns>The id of the created account.</returns>
		Task<Guid> CreateAccountAsync(AccountCreationModel account);

		/// <summary>
		/// Gets all currencies.
		/// </summary>
		/// <returns>A collection with all currencies.</returns>
		Task<List<CurrencyModel>> GetCurrenciesAsync();
	}
}
