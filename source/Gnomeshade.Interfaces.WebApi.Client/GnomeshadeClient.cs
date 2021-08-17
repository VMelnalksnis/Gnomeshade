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
		public async Task<LoginResult> LogInAsync(Login login)
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

		/// <inheritdoc />
		public Task<Counterparty> GetMyCounterpartyAsync() =>
			GetAsync<Counterparty>("Counterparty/Me");

		/// <inheritdoc/>
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction) =>
			PostAsync(TransactionUri, transaction);

		/// <inheritdoc />
		public Task<Guid> PutTransactionAsync(TransactionCreationModel transaction) =>
			PutAsync(TransactionUri, transaction);

		/// <inheritdoc />
		public Task<Guid> PutTransactionItemAsync(Guid transactionId, TransactionItemCreationModel item) =>
			PutAsync(TransactionItemUri(transactionId), item);

		/// <inheritdoc />
		public Task<Transaction> GetTransactionAsync(Guid id) =>
			GetAsync<Transaction>(TransactionIdUri(id));

		/// <inheritdoc />
		public Task<TransactionItem> GetTransactionItemAsync(Guid id) =>
			GetAsync<TransactionItem>(TransactionItemIdUri(id));

		/// <inheritdoc />
		public Task<List<Transaction>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to) =>
			GetAsync<List<Transaction>>(TransactionDateRangeUri(from, to));

		/// <inheritdoc />
		public Task DeleteTransactionAsync(Guid id) =>
			DeleteAsync(TransactionIdUri(id));

		/// <inheritdoc />
		public Task DeleteTransactionItemAsync(Guid id) =>
			DeleteAsync(TransactionItemIdUri(id));

		/// <inheritdoc />
		public Task<Account> GetAccountAsync(Guid id) =>
			GetAsync<Account>(AccountIdUri(id));

		/// <inheritdoc />
		public Task<Account?> FindAccountAsync(string name) =>
			FindAsync<Account>(AccountNameUri(name));

		/// <inheritdoc />
		public Task<List<Account>> GetAccountsAsync() =>
			GetAsync<List<Account>>(AllAccountUri);

		/// <inheritdoc />
		public Task<List<Account>> GetActiveAccountsAsync() =>
			GetAsync<List<Account>>(AccountUri);

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account) =>
			PostAsync(AccountUri, account);

		/// <inheritdoc />
		public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency) =>
			PostAsync(AccountIdUri(id), currency);

		/// <inheritdoc />
		public Task<List<Currency>> GetCurrenciesAsync() =>
			GetAsync<List<Currency>>(CurrencyUri);

		/// <inheritdoc />
		public Task<List<Product>> GetProductsAsync() =>
			GetAsync<List<Product>>(ProductUri);

		/// <inheritdoc />
		public Task<Product> GetProductAsync(Guid id) =>
			GetAsync<Product>(ProductIdUri(id));

		/// <inheritdoc />
		public Task<List<Unit>> GetUnitsAsync() =>
			GetAsync<List<Unit>>(UnitUri);

		/// <inheritdoc />
		public Task<Guid> PutProductAsync(ProductCreationModel product) =>
			PutAsync(ProductUri, product);

		/// <inheritdoc />
		public Task<Guid> CreateUnitAsync(UnitCreationModel unit) =>
			PostAsync(UnitUri, unit);

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

		private async Task<TResult?> FindAsync<TResult>(string requestUri)
			where TResult : class
		{
			using var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}

			await ThrowIfNotSuccessCode(response);
			return (await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false))!;
		}

		private async Task<Guid> PostAsync<TRequest>(string requestUri, TRequest request)
			where TRequest : notnull
		{
			using var response = await _httpClient.PostAsJsonAsync(requestUri, request).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(response);

			return await response.Content.ReadFromJsonAsync<Guid>().ConfigureAwait(false);
		}

		private async Task<Guid> PutAsync<TRequest>(string requestUri, TRequest request)
			where TRequest : notnull
		{
			using var response = await _httpClient.PutAsJsonAsync(requestUri, request).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(response);

			return await response.Content.ReadFromJsonAsync<Guid>().ConfigureAwait(false);
		}

		private async Task DeleteAsync(string requestUri)
		{
			var deleteResponse = await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(deleteResponse);
		}
	}
}
