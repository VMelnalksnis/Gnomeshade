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
		public async Task<UserModel> InfoAsync()
		{
			return await GetAsync<UserModel>(InfoUri).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction)
		{
			return await PostAsync<Guid, TransactionCreationModel>(Transaction, transaction).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<TransactionModel>> GetTransactionsAsync()
		{
			return await GetAsync<List<TransactionModel>>(Transaction).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return await GetAsync<List<TransactionModel>>(TransactionUri(from, to)).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<AccountModel> GetAccountAsync(Guid id)
		{
			return await GetAsync<AccountModel>(AccountUri(id)).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<AccountModel>> GetAccountsAsync()
		{
			return await GetAsync<List<AccountModel>>(Account).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> CreateAccountAsync(AccountCreationModel account)
		{
			return await PostAsync<Guid, AccountCreationModel>(Account, account).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<CurrencyModel>> GetCurrenciesAsync()
		{
			return await GetAsync<List<CurrencyModel>>(Currency).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<ProductModel>> GetProductsAsync()
		{
			return await GetAsync<List<ProductModel>>(Product).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<List<UnitModel>> GetUnitsAsync()
		{
			return await GetAsync<List<UnitModel>>(Unit).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> CreateProductAsync(ProductCreationModel product)
		{
			return await PostAsync<Guid, ProductCreationModel>(Product, product).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> CreateUnitAsync(UnitCreationModel unit)
		{
			return await PostAsync<Guid, UnitCreationModel>(Unit, unit).ConfigureAwait(false);
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
