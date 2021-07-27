// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client.Login;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using static Gnomeshade.Interfaces.WebApi.Client.Routes;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <inheritdoc cref="IGnomeshadeClient"/>
	public sealed class GnomeshadeClient : IGnomeshadeClient
	{
		private readonly HttpClient _httpClient = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="GnomeshadeClient"/> class.
		/// </summary>
		public GnomeshadeClient()
			: this(new("https://localhost:5001/api/v1.0/"))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GnomeshadeClient"/> class with a base uri.
		/// </summary>
		/// <param name="baseUri">The base uri for all requests.</param>
		/// <see cref="HttpClient.BaseAddress"/>
		public GnomeshadeClient(Uri baseUri)
		{
			_httpClient.BaseAddress = baseUri;
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
		}

		/// <inheritdoc/>
		public async Task<LoginResult> LogInAsync(LoginModel login)
		{
			try
			{
				using var response = await _httpClient.PostAsJsonAsync(LoginUri, login).ConfigureAwait(false);
				if (response.IsSuccessStatusCode)
				{
					var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>().ConfigureAwait(false);
					_httpClient.DefaultRequestHeaders.Authorization =
						new(JwtBearerDefaults.AuthenticationScheme, loginResponse!.Token);
					return new SuccessfulLogin(loginResponse);
				}

				var errorResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				return new FailedLogin(response.StatusCode, errorResponse);
			}
			catch (HttpRequestException httpException)
			{
				return new FailedLogin(httpException.StatusCode, httpException.Message);
			}
		}

		/// <inheritdoc />
		public Task LogOutAsync()
		{
			_httpClient.DefaultRequestHeaders.Authorization = null;
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public Task<UserModel> InfoAsync()
		{
			return GetAsync<UserModel>(InfoUri);
		}

		/// <inheritdoc/>
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction)
		{
			return PostAsync<Guid, TransactionCreationModel>(Transaction, transaction);
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync()
		{
			return GetAsync<List<TransactionModel>>(Transaction);
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return GetAsync<List<TransactionModel>>(TransactionUri(from, to));
		}

		/// <inheritdoc />
		public Task<AccountModel> GetAccountAsync(Guid id)
		{
			return GetAsync<AccountModel>(AccountUri(id));
		}

		/// <inheritdoc />
		public Task<List<AccountModel>> GetAccountsAsync()
		{
			return GetAsync<List<AccountModel>>(Account);
		}

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account)
		{
			return PostAsync<Guid, AccountCreationModel>(Account, account);
		}

		/// <inheritdoc />
		public Task<List<CurrencyModel>> GetCurrenciesAsync()
		{
			return GetAsync<List<CurrencyModel>>(Currency);
		}

		/// <inheritdoc />
		public Task<List<ProductModel>> GetProductsAsync()
		{
			return GetAsync<List<ProductModel>>(Product);
		}

		/// <inheritdoc />
		public Task<List<UnitModel>> GetUnitsAsync()
		{
			return GetAsync<List<UnitModel>>(Unit);
		}

		/// <inheritdoc />
		public Task<Guid> CreateProductAsync(ProductCreationModel product)
		{
			return PostAsync<Guid, ProductCreationModel>(Product, product);
		}

		/// <inheritdoc />
		public Task<Guid> CreateUnitAsync(UnitCreationModel unit)
		{
			return PostAsync<Guid, UnitCreationModel>(Unit, unit);
		}

		private async Task<TResult> GetAsync<TResult>(string requestUri)
			where TResult : notnull
		{
			using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task<TResult> PostAsync<TResult, TRequest>(string requestUri, TRequest request)
			where TResult : notnull
			where TRequest : notnull
		{
			using var response = await _httpClient.PostAsJsonAsync(requestUri, request).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}
	}
}
