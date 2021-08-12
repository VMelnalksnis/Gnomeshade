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
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results;
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
		public Task<CounterpartyModel> GetMyCounterpartyAsync()
		{
			return GetAsync<CounterpartyModel>("Counterparty/Me");
		}

		/// <inheritdoc/>
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction)
		{
			return PostAsync<Guid, TransactionCreationModel>(Transaction, transaction);
		}

		/// <inheritdoc />
		public Task<Guid> AddTransactionItemAsync(Guid transactionId, TransactionItemCreationModel item)
		{
			return PostAsync<Guid, TransactionItemCreationModel>($"{TransactionUri(transactionId)}/Item", item);
		}

		/// <inheritdoc />
		public Task<TransactionModel> GetTransactionAsync(Guid id)
		{
			return GetAsync<TransactionModel>(TransactionUri(id));
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return GetAsync<List<TransactionModel>>(TransactionUri(from, to));
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
		public Task<AccountModel> GetAccountAsync(Guid id)
		{
			return GetAsync<AccountModel>(AccountUri(id));
		}

		/// <inheritdoc />
		public async Task<AccountModel?> FindAccountAsync(string name)
		{
			using var response = await _httpClient.GetAsync($"Account/Find/{name}").ConfigureAwait(false);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}

			await ThrowIfNotSuccessCode(response);
			return (await response.Content.ReadFromJsonAsync<AccountModel>().ConfigureAwait(false))!;
		}

		/// <inheritdoc />
		public Task<List<AccountModel>> GetAccountsAsync()
		{
			return GetAsync<List<AccountModel>>($"{Account}?onlyActive=false");
		}

		/// <inheritdoc />
		public Task<List<AccountModel>> GetActiveAccountsAsync()
		{
			return GetAsync<List<AccountModel>>(Account);
		}

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account)
		{
			return PostAsync<Guid, AccountCreationModel>(Account, account);
		}

		/// <inheritdoc />
		public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency)
		{
			return PostAsync<Guid, AccountInCurrencyCreationModel>(AccountUri(id), currency);
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

		private async Task DeleteAsync(string requestUri)
		{
			var deleteResponse = await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
			await ThrowIfNotSuccessCode(deleteResponse);
		}

		private async Task ThrowIfNotSuccessCode(HttpResponseMessage responseMessage)
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
	}
}
