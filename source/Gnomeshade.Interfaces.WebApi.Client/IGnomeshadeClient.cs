// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client.Login;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the API provided by the Interfaces.WebApi project.
	/// </summary>
	public interface IGnomeshadeClient
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
		/// Gets the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to get.</param>
		/// <returns>The transaction with the specified id.</returns>
		Task<TransactionModel> GetTransactionAsync(Guid id);

		/// <summary>
		/// Gets all transactions within the specified time period.
		/// </summary>
		/// <param name="from">The time from which to get transactions.</param>
		/// <param name="to">The time until which to get transactions.</param>
		/// <returns>All transactions within the specified time period.</returns>
		Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to);

		/// <summary>
		/// Deletes the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to delete.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeleteTransactionAsync(Guid id);

		/// <summary>
		/// Deletes the specified transaction item.
		/// </summary>
		/// <param name="id">The id of the transaction item to delete.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeleteTransactionItemAsync(Guid id);

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
		Task<List<CurrencyModel>> GetCurrenciesAsync();

		/// <summary>
		/// Gets all products.
		/// </summary>
		/// <returns>A collection with all products.</returns>
		Task<List<ProductModel>> GetProductsAsync();

		/// <summary>
		/// Gets all units.
		/// </summary>
		/// <returns>A collection with all units.</returns>
		Task<List<UnitModel>> GetUnitsAsync();

		/// <summary>
		/// Creates a new product.
		/// </summary>
		/// <param name="product">Information for creating the product.</param>
		/// <returns>The id of the created product.</returns>
		Task<Guid> CreateProductAsync(ProductCreationModel product);

		Task<Guid> CreateUnitAsync(UnitCreationModel unit);
	}
}
