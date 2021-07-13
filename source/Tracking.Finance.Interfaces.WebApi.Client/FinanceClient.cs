// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Tracking.Finance.Interfaces.WebApi.Client.Login;
using Tracking.Finance.Interfaces.WebApi.V1_0.Accounts;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Products;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

using static Tracking.Finance.Interfaces.WebApi.Client.Routes;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	/// <inheritdoc cref="IFinanceClient"/>
	public sealed class FinanceClient : IFinanceClient
	{
		private readonly HttpClient _httpClient = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="FinanceClient"/> class.
		/// </summary>
		public FinanceClient()
			: this(new("https://localhost:5001/api/v1.0/"))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FinanceClient"/> class with a base uri.
		/// </summary>
		///
		/// <param name="baseUri">The base uri for all requests.</param>
		/// <see cref="HttpClient.BaseAddress"/>
		public FinanceClient(Uri baseUri)
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
					_httpClient.DefaultRequestHeaders.Authorization = new(JwtBearerDefaults.AuthenticationScheme, loginResponse!.Token);
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

		private async Task<TResult> GetAsync<TResult>(string requestUri)
			where TResult : notnull
		{
			using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task<TResult?> FindAsync<TResult>(string requestUri)
		{
			using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
			}

			return default;
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
