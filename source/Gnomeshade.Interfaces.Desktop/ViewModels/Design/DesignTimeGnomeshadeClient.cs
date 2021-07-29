// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Client.Login;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Design
{
	/// <summary>
	/// An implementation of <see cref="IGnomeshadeClient"/> for use during design time.
	/// </summary>
	public sealed class DesignTimeGnomeshadeClient : IGnomeshadeClient
	{
		/// <inheritdoc />
		public Task<LoginResult> LogInAsync(LoginModel login) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task LogOutAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<UserModel> InfoAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync()
		{
			return Task.FromResult(new List<TransactionModel>());
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return Task.FromResult(new List<TransactionModel>());
		}

		/// <inheritdoc />
		public Task DeleteTransactionAsync(Guid id) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<AccountModel> GetAccountAsync(Guid id) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<AccountModel>> GetAccountsAsync()
		{
			return Task.FromResult(new List<AccountModel>());
		}

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<CurrencyModel>> GetCurrenciesAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<ProductModel>> GetProductsAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<UnitModel>> GetUnitsAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> CreateProductAsync(ProductCreationModel product) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> CreateUnitAsync(UnitCreationModel unit) => throw new NotImplementedException();
	}
}
