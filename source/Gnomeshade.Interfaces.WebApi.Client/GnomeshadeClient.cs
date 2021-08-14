// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client.Login;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

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
			: this(new Uri("https://localhost:5001/api/v1.0/"))
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

		/// <summary>
		/// Initializes a new instance of the <see cref="GnomeshadeClient"/> class.
		/// </summary>
		/// <param name="httpClient">The HTTP client to use for requests.</param>
		public GnomeshadeClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			if (httpClient.BaseAddress?.AbsolutePath == "/")
			{
				var uriBuilder = new UriBuilder(httpClient.BaseAddress);
				uriBuilder.Path += "api/v1.0/";
				_httpClient.BaseAddress = uriBuilder.Uri;
			}

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

		/// <inheritdoc />
		public Task<Counterparty> GetMyCounterpartyAsync()
		{
			return GetAsync<Counterparty>("Counterparty/Me");
		}

		/// <inheritdoc/>
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction)
		{
			return PostAsync<Guid, TransactionCreationModel>(Routes.Transaction, transaction);
		}

		/// <inheritdoc />
		public Task<Guid> PutTransactionItemAsync(Guid transactionId, TransactionItemCreationModel item)
		{
			return PutAsync<Guid, TransactionItemCreationModel>($"{TransactionUri(transactionId)}/Item", item);
		}

		/// <inheritdoc />
		public Task<Transaction> GetTransactionAsync(Guid id)
		{
			return GetAsync<Transaction>(TransactionUri(id));
		}

		/// <inheritdoc />
		public Task<List<Transaction>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return GetAsync<List<Transaction>>(TransactionUri(from, to));
		}

		/// <inheritdoc />
		public Task DeleteTransactionAsync(Guid id)
		{
			return DeleteAsync(TransactionUri(id));
		}

		/// <inheritdoc />
		public Task DeleteTransactionItemAsync(Guid id)
		{
			return DeleteAsync(TransactionItemUri(id));
		}

		/// <inheritdoc />
		public Task<Account> GetAccountAsync(Guid id)
		{
			return GetAsync<Account>(AccountUri(id));
		}

		/// <inheritdoc />
		public async Task<Account?> FindAccountAsync(string name)
		{
			using var response = await _httpClient.GetAsync($"Account/Find/{name}").ConfigureAwait(false);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}

			await ThrowIfNotSuccessCode(response);
			return (await response.Content.ReadFromJsonAsync<Account>().ConfigureAwait(false))!;
		}

		/// <inheritdoc />
		public Task<List<Account>> GetAccountsAsync()
		{
			return GetAsync<List<Account>>($"{Routes.Account}?onlyActive=false");
		}

		/// <inheritdoc />
		public Task<List<Account>> GetActiveAccountsAsync()
		{
			return GetAsync<List<Account>>(Routes.Account);
		}

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account)
		{
			return PostAsync<Guid, AccountCreationModel>(Routes.Account, account);
		}

		/// <inheritdoc />
		public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency)
		{
			return PostAsync<Guid, AccountInCurrencyCreationModel>(AccountUri(id), currency);
		}

		/// <inheritdoc />
		public Task<List<Currency>> GetCurrenciesAsync()
		{
			return GetAsync<List<Currency>>(Routes.Currency);
		}

		/// <inheritdoc />
		public Task<List<Product>> GetProductsAsync()
		{
			return GetAsync<List<Product>>(Routes.Product);
		}

		/// <inheritdoc />
		public Task<List<Unit>> GetUnitsAsync()
		{
			return GetAsync<List<Unit>>(Routes.Unit);
		}

		/// <inheritdoc />
		public Task<Guid> PutProductAsync(ProductCreationModel product)
		{
			return PutAsync<Guid, ProductCreationModel>(Routes.Product, product);
		}

		/// <inheritdoc />
		public Task<Guid> CreateUnitAsync(UnitCreationModel unit)
		{
			return PostAsync<Guid, UnitCreationModel>(Routes.Unit, unit);
		}

		/// <inheritdoc />
		public async Task<AccountReportResult> Import(Stream content, string name)
		{
			var streamContent = new StreamContent(content);
			streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
			var multipartContent = new MultipartFormDataContent();
			multipartContent.Add(streamContent, "formFile", name);

			using var importResponse = await _httpClient.PostAsync(Iso20022, multipartContent);
			await ThrowIfNotSuccessCode(importResponse);

			return (await importResponse.Content.ReadFromJsonAsync<AccountReportResult>())!;
		}

		private static async Task ThrowIfNotSuccessCode(HttpResponseMessage responseMessage)
		{
			try
			{
				responseMessage.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException requestException)
			{
				var message = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
				throw new HttpRequestException(
					$"Failed with message: {message}",
					requestException,
					responseMessage.StatusCode);
			}
		}

		private async Task<TResult> GetAsync<TResult>(string requestUri)
			where TResult : notnull
		{
			using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(response);

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task<TResult> PostAsync<TResult, TRequest>(string requestUri, TRequest request)
			where TResult : notnull
			where TRequest : notnull
		{
			using var response = await _httpClient.PostAsJsonAsync(requestUri, request).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(response);

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task<TResult> PutAsync<TResult, TRequest>(string requestUri, TRequest request)
			where TResult : notnull
			where TRequest : notnull
		{
			using var response = await _httpClient.PutAsJsonAsync(requestUri, request).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(response);

			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task DeleteAsync(string requestUri)
		{
			var deleteResponse = await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(deleteResponse);
		}
	}
}
