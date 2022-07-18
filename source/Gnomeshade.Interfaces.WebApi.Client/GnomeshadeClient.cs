﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Owners;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

using static Gnomeshade.Interfaces.WebApi.Client.Routes;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <inheritdoc cref="IGnomeshadeClient"/>
public sealed class GnomeshadeClient : IGnomeshadeClient
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of the <see cref="GnomeshadeClient"/> class.</summary>
	/// <param name="httpClient">The HTTP client to use for requests.</param>
	/// <param name="gnomeshadeJsonSerializerOptions">Gnomeshade specific instance of <see cref="JsonSerializerOptions"/>.</param>
	public GnomeshadeClient(HttpClient httpClient, GnomeshadeJsonSerializerOptions gnomeshadeJsonSerializerOptions)
	{
		_httpClient = httpClient;
		_httpClient.DefaultRequestHeaders.Accept.Clear();
		_httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));

		_jsonSerializerOptions = gnomeshadeJsonSerializerOptions.Options;
	}

	/// <inheritdoc/>
	public async Task<LoginResult> LogInAsync(Login login)
	{
		try
		{
			using var response = await _httpClient.PostAsJsonAsync(_loginUri, login, _jsonSerializerOptions).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonSerializerOptions).ConfigureAwait(false);
				_httpClient.DefaultRequestHeaders.Authorization = new("Bearer", loginResponse!.Token);
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
	public async Task SocialRegister()
	{
		using var response = await _httpClient.PostAsync(_socialRegisterUri, new StringContent(string.Empty));
		response.EnsureSuccessStatusCode();
	}

	/// <inheritdoc />
	public async Task LogOutAsync()
	{
		_httpClient.DefaultRequestHeaders.Authorization = null;
		using var response =
			await _httpClient.PostAsync(_logOutUri, new StringContent(string.Empty)).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public Task<List<Link>> GetLinksAsync(CancellationToken cancellationToken = default) =>
		GetAsync<List<Link>>(Links._uri, cancellationToken);

	/// <inheritdoc />
	public Task<Link> GetLinkAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<Link>(Links.IdUri(id), cancellationToken);

	/// <inheritdoc />
	public Task PutLinkAsync(Guid id, LinkCreation link) =>
		PutAsync(Links.IdUri(id), link);

	/// <inheritdoc />
	public Task DeleteLinkAsync(Guid id) =>
		DeleteAsync(Links.IdUri(id));

	/// <inheritdoc />
	public Task<Counterparty> GetMyCounterpartyAsync() =>
		GetAsync<Counterparty>($"{_counterpartyUri}/Me");

	/// <inheritdoc />
	public Task<Counterparty> GetCounterpartyAsync(Guid id) =>
		GetAsync<Counterparty>(CounterpartyIdUri(id));

	/// <inheritdoc />
	public Task<List<Counterparty>> GetCounterpartiesAsync() =>
		GetAsync<List<Counterparty>>(_counterpartyUri);

	/// <inheritdoc />
	public Task<Guid> CreateCounterpartyAsync(CounterpartyCreation counterparty) =>
		PostAsync(_counterpartyUri, counterparty);

	/// <inheritdoc />
	public Task PutCounterpartyAsync(Guid id, CounterpartyCreation counterparty) =>
		PutAsync(CounterpartyIdUri(id), counterparty);

	/// <inheritdoc />
	public async Task MergeCounterpartiesAsync(Guid targetId, Guid sourceId)
	{
		using var response = await _httpClient.PostAsync(CounterpartyMergeUri(targetId, sourceId), null);
		await ThrowIfNotSuccessCode(response);
	}

	/// <inheritdoc/>
	public Task<Guid> CreateTransactionAsync(TransactionCreation transaction) =>
		PostAsync(Transactions.Uri, transaction);

	/// <inheritdoc />
	public Task PutTransactionAsync(Guid id, TransactionCreation transaction) =>
		PutAsync(Transactions.IdUri(id), transaction);

	/// <inheritdoc />
	public Task<Transaction> GetTransactionAsync(Guid id) =>
		GetAsync<Transaction>(Transactions.IdUri(id));

	/// <inheritdoc />
	public Task<DetailedTransaction> GetDetailedTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<DetailedTransaction>(Transactions.DetailedIdUri(id), cancellationToken);

	/// <inheritdoc />
	public Task<List<Transaction>> GetTransactionsAsync(Instant? from, Instant? to) =>
		GetAsync<List<Transaction>>(Transactions.DateRangeUri(from, to));

	/// <inheritdoc />
	public Task<List<DetailedTransaction>> GetDetailedTransactionsAsync(
		Instant? from,
		Instant? to,
		CancellationToken cancellationToken = default) =>
		GetAsync<List<DetailedTransaction>>(Transactions.DetailedDateRangeUri(from, to), cancellationToken);

	/// <inheritdoc />
	public Task DeleteTransactionAsync(Guid id) =>
		DeleteAsync(Transactions.IdUri(id));

	/// <inheritdoc />
	public Task<List<Link>> GetTransactionLinksAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync<List<Link>>(Transactions.LinkUri(transactionId), cancellationToken);

	/// <inheritdoc />
	public Task AddLinkToTransactionAsync(Guid transactionId, Guid linkId) =>
		PutAsync(Transactions.LinkIdUri(transactionId, linkId));

	/// <inheritdoc />
	public Task RemoveLinkFromTransactionAsync(Guid transactionId, Guid linkId) =>
		DeleteAsync(Transactions.LinkIdUri(transactionId, linkId));

	/// <inheritdoc />
	public Task<List<Transfer>> GetTransfersAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync<List<Transfer>>(Transfers.Uri(transactionId), cancellationToken);

	/// <inheritdoc />
	public Task<Transfer> GetTransferAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<Transfer>(Transfers.IdUri(transactionId, id), cancellationToken);

	/// <inheritdoc />
	public Task PutTransferAsync(Guid transactionId, Guid id, TransferCreation transfer) =>
		PutAsync(Transfers.IdUri(transactionId, id), transfer);

	/// <inheritdoc />
	public Task DeleteTransferAsync(Guid transactionId, Guid id) =>
		DeleteAsync(Transfers.IdUri(transactionId, id));

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync<List<Purchase>>(Purchases.Uri(transactionId), cancellationToken);

	/// <inheritdoc />
	public Task<Purchase> GetPurchaseAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<Purchase>(Purchases.IdUri(transactionId, id), cancellationToken);

	/// <inheritdoc />
	public Task PutPurchaseAsync(Guid transactionId, Guid id, PurchaseCreation purchase) =>
		PutAsync(Purchases.IdUri(transactionId, id), purchase);

	/// <inheritdoc />
	public Task DeletePurchaseAsync(Guid transactionId, Guid id) =>
		DeleteAsync(Purchases.IdUri(transactionId, id));

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync<List<Loan>>(Loans.Uri(transactionId), cancellationToken);

	/// <inheritdoc />
	public Task<List<Loan>> GetCounterpartyLoansAsync(Guid counterpartyId, CancellationToken cancellationToken = default) =>
		GetAsync<List<Loan>>(Loans.CounterpartyUri(counterpartyId), cancellationToken);

	/// <inheritdoc />
	public Task<Loan> GetLoanAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<Loan>(Loans.IdUri(transactionId, id), cancellationToken);

	/// <inheritdoc />
	public Task PutLoanAsync(Guid transactionId, Guid id, LoanCreation loan) =>
		PutAsync(Loans.IdUri(transactionId, id), loan);

	/// <inheritdoc />
	public Task DeleteLoanAsync(Guid transactionId, Guid id) =>
		DeleteAsync(Loans.IdUri(transactionId, id));

	/// <inheritdoc />
	public Task<Account> GetAccountAsync(Guid id) =>
		GetAsync<Account>(Accounts.IdUri(id));

	/// <inheritdoc />
	public Task<List<Account>> GetAccountsAsync() =>
		GetAsync<List<Account>>(Accounts._allUri);

	/// <inheritdoc />
	public Task<List<Account>> GetActiveAccountsAsync() =>
		GetAsync<List<Account>>(Accounts._uri);

	/// <inheritdoc />
	public Task<Guid> CreateAccountAsync(AccountCreation account) =>
		PostAsync(Accounts._uri, account);

	/// <inheritdoc />
	public Task PutAccountAsync(Guid id, AccountCreation account) =>
		PutAsync(Accounts.IdUri(id), account);

	/// <inheritdoc />
	public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreation currency) =>
		PostAsync(Accounts.CurrencyUri(id), currency);

	/// <inheritdoc />
	public Task RemoveCurrencyFromAccountAsync(Guid id, Guid currencyId) =>
		DeleteAsync(Accounts.CurrencyIdUri(id, currencyId));

	/// <inheritdoc />
	public Task<List<Currency>> GetCurrenciesAsync() =>
		GetAsync<List<Currency>>(_currencyUri);

	/// <inheritdoc />
	public Task<List<Balance>> GetAccountBalanceAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<List<Balance>>(Accounts.BalanceUri(id), cancellationToken);

	/// <inheritdoc />
	public Task<List<Product>> GetProductsAsync() =>
		GetAsync<List<Product>>(Products.Uri);

	/// <inheritdoc />
	public Task<Product> GetProductAsync(Guid id) =>
		GetAsync<Product>(Products.IdUri(id));

	/// <inheritdoc />
	public Task<Unit> GetUnitAsync(Guid id) =>
		GetAsync<Unit>(UnitIdUri(id));

	/// <inheritdoc />
	public Task<List<Unit>> GetUnitsAsync() =>
		GetAsync<List<Unit>>(_unitUri);

	/// <inheritdoc />
	public Task PutProductAsync(Guid id, ProductCreation product) =>
		PutAsync(Products.IdUri(id), product);

	/// <inheritdoc />
	public Task PutUnitAsync(Guid id, UnitCreation unit) =>
		PutAsync(UnitIdUri(id), unit);

	/// <inheritdoc />
	public async Task<AccountReportResult> Import(Stream content, string name)
	{
		var streamContent = new StreamContent(content);
		streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
		var multipartContent = new MultipartFormDataContent();
		multipartContent.Add(streamContent, "Report", name);
		multipartContent.Add(new StringContent(DateTimeZoneProviders.Tzdb.GetSystemDefault().Id), "TimeZone");

		using var importResponse = await _httpClient.PostAsync(_iso20022, multipartContent);
		await ThrowIfNotSuccessCode(importResponse);

		return (await importResponse.Content.ReadFromJsonAsync<AccountReportResult>(_jsonSerializerOptions))!;
	}

	/// <inheritdoc />
	public Task<List<string>> GetInstitutionsAsync(string countryCode, CancellationToken cancellationToken = default) =>
		GetAsync<List<string>>(Nordigen.Institutions(countryCode), cancellationToken);

	/// <inheritdoc />
	public async Task<List<AccountReportResult>> ImportAsync(string id)
	{
		var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault().Id;
		using var importResponse = await _httpClient.PostAsync(Nordigen.Import(id, timeZone), null);
		await ThrowIfNotSuccessCode(importResponse);

		return (await importResponse.Content.ReadFromJsonAsync<List<AccountReportResult>>(_jsonSerializerOptions))!;
	}

	/// <inheritdoc />
	public Task<List<Category>> GetCategoriesAsync() => GetAsync<List<Category>>(Categories._uri);

	/// <inheritdoc />
	public Task<Category> GetCategoryAsync(Guid id) => GetAsync<Category>(Categories.IdUri(id));

	/// <inheritdoc />
	public Task<Guid> CreateCategoryAsync(CategoryCreation category) => PostAsync(Categories._uri, category);

	/// <inheritdoc />
	public Task PutCategoryAsync(Guid id, CategoryCreation category) => PutAsync(Categories.IdUri(id), category);

	/// <inheritdoc />
	public Task DeleteCategoryAsync(Guid id) => DeleteAsync(Categories.IdUri(id));

	/// <inheritdoc />
	public Task<List<Purchase>> GetProductPurchasesAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync<List<Purchase>>(Products.PurchasesUri(id), cancellationToken);

	/// <inheritdoc />
	public Task<List<Access>> GetAccessesAsync(CancellationToken cancellationToken = default) =>
		GetAsync<List<Access>>("Access", cancellationToken);

	/// <inheritdoc />
	public Task DeleteOwnerAsync(Guid id) =>
		DeleteAsync(Owners.IdUri(id));

	/// <inheritdoc />
	public Task<List<Ownership>> GetOwnershipsAsync(CancellationToken cancellationToken = default) =>
		GetAsync<List<Ownership>>(Ownerships.Uri, cancellationToken);

	/// <inheritdoc />
	public Task<List<Owner>> GetOwnersAsync(CancellationToken cancellationToken = default) =>
		GetAsync<List<Owner>>(Owners.Uri, cancellationToken);

	/// <inheritdoc />
	public Task PutOwnerAsync(Guid id) =>
		PutAsync(Owners.IdUri(id));

	/// <inheritdoc />
	public Task PutOwnershipAsync(Guid id, OwnershipCreation ownership) =>
		PutAsync(Ownerships.IdUri(id), ownership);

	/// <inheritdoc />
	public Task DeleteOwnershipAsync(Guid id) =>
		DeleteAsync(Ownerships.IdUri(id));

	private static async Task ThrowIfNotSuccessCode(HttpResponseMessage responseMessage)
	{
		if (responseMessage.IsSuccessStatusCode)
		{
			return;
		}

		var message = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
		throw new HttpRequestException($"Failed with message: {message}", null, responseMessage.StatusCode);
	}

	private async Task<TResult> GetAsync<TResult>(string requestUri, CancellationToken cancellationToken = default)
		where TResult : notnull
	{
		using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);

		return (await response.Content.ReadFromJsonAsync<TResult>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false))!;
	}

	private async Task<Guid> PostAsync<TRequest>(string requestUri, TRequest request)
		where TRequest : notnull
	{
		using var response = await _httpClient.PostAsJsonAsync(requestUri, request, _jsonSerializerOptions).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);

		return await response.Content.ReadFromJsonAsync<Guid>(_jsonSerializerOptions).ConfigureAwait(false);
	}

	private async Task PutAsync(string requestUri)
	{
		using var response = await _httpClient.PutAsync(requestUri, null).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);
	}

	private async Task PutAsync<TRequest>(string requestUri, TRequest request)
		where TRequest : notnull
	{
		using var response = await _httpClient.PutAsJsonAsync(requestUri, request, _jsonSerializerOptions).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);
	}

	private async Task DeleteAsync(string requestUri)
	{
		var deleteResponse = await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(deleteResponse);
	}
}
