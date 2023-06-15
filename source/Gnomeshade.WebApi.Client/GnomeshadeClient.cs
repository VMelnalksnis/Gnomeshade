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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client.Results;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Authentication;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using static Gnomeshade.WebApi.Client.Routes;

namespace Gnomeshade.WebApi.Client;

/// <inheritdoc cref="IGnomeshadeClient"/>
public sealed class GnomeshadeClient : IGnomeshadeClient
{
	private readonly GnomeshadeSerializerContext _context;
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of the <see cref="GnomeshadeClient"/> class.</summary>
	/// <param name="httpClient">The HTTP client to use for requests.</param>
	/// <param name="gnomeshadeJsonSerializerOptions">Gnomeshade specific instance of <see cref="JsonSerializerOptions"/>.</param>
	public GnomeshadeClient(HttpClient httpClient, GnomeshadeJsonSerializerOptions gnomeshadeJsonSerializerOptions)
	{
		_httpClient = httpClient;
		_httpClient.DefaultRequestHeaders.Accept.Clear();
		_httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));

		_context = gnomeshadeJsonSerializerOptions.Context;
	}

	/// <inheritdoc/>
	public async Task<LoginResult> LogInAsync(Login login)
	{
		try
		{
			using var response = await _httpClient.PostAsJsonAsync(_loginUri, login, _context.Login).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				return new SuccessfulLogin();
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
	public async Task<ExternalLoginResult> SocialRegister()
	{
		using var response = await _httpClient.PostAsync(_socialRegisterUri, null);
		if (response.StatusCode is HttpStatusCode.NoContent)
		{
			return new LoggedIn();
		}

		if (response.StatusCode is HttpStatusCode.Redirect && response.Headers.Location is { } location)
		{
			return new RequiresRegistration(location);
		}

		var message = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		throw new HttpRequestException($"{response.StatusCode}; Details: '{message}'", null, response.StatusCode);
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
		GetAsync(Links._uri, _context.ListLink, cancellationToken);

	/// <inheritdoc />
	public Task<Link> GetLinkAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Links.IdUri(id), _context.Link, cancellationToken);

	/// <inheritdoc />
	public Task PutLinkAsync(Guid id, LinkCreation link) =>
		PutAsync(Links.IdUri(id), link, _context.LinkCreation);

	/// <inheritdoc />
	public Task DeleteLinkAsync(Guid id) =>
		DeleteAsync(Links.IdUri(id));

	/// <inheritdoc />
	public Task<Counterparty> GetMyCounterpartyAsync(CancellationToken cancellationToken = default) =>
		GetAsync($"{_counterpartyUri}/Me", _context.Counterparty, cancellationToken);

	/// <inheritdoc />
	public Task<Counterparty> GetCounterpartyAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(CounterpartyIdUri(id), _context.Counterparty, cancellationToken);

	/// <inheritdoc />
	public Task<List<Counterparty>> GetCounterpartiesAsync(CancellationToken cancellationToken = default) =>
		GetAsync(_counterpartyUri, _context.ListCounterparty, cancellationToken);

	/// <inheritdoc />
	public Task<Guid> CreateCounterpartyAsync(CounterpartyCreation counterparty) =>
		PostAsync(_counterpartyUri, counterparty, _context.CounterpartyCreation);

	/// <inheritdoc />
	public Task PutCounterpartyAsync(Guid id, CounterpartyCreation counterparty) =>
		PutAsync(CounterpartyIdUri(id), counterparty, _context.CounterpartyCreation);

	/// <inheritdoc />
	public Task MergeCounterpartiesAsync(Guid targetId, Guid sourceId) =>
		PostAsync(CounterpartyMergeUri(targetId, sourceId));

	/// <inheritdoc/>
	public Task<Guid> CreateTransactionAsync(TransactionCreation transaction) =>
		PostAsync(Transactions.Uri, transaction, _context.TransactionCreation);

	/// <inheritdoc />
	public Task PutTransactionAsync(Guid id, TransactionCreation transaction) =>
		PutAsync(Transactions.IdUri(id), transaction, _context.TransactionCreation);

	/// <inheritdoc />
	public Task<Transaction> GetTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.IdUri(id), _context.Transaction, cancellationToken);

	/// <inheritdoc />
	public Task<DetailedTransaction> GetDetailedTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.DetailedIdUri(id), _context.DetailedTransaction, cancellationToken);

	/// <inheritdoc />
	public Task<List<Transaction>> GetTransactionsAsync(
		CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.Uri, _context.ListTransaction, cancellationToken);

	/// <inheritdoc />
	public Task<List<DetailedTransaction>> GetDetailedTransactionsAsync(
		Interval interval,
		CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.DetailedDateRangeUri(interval), _context.ListDetailedTransaction, cancellationToken);

	/// <inheritdoc />
	public Task DeleteTransactionAsync(Guid id) =>
		DeleteAsync(Transactions.IdUri(id));

	/// <inheritdoc />
	public Task MergeTransactionsAsync(Guid targetId, Guid sourceId) =>
		PostAsync(Transactions.MergeUri(targetId, sourceId));

	/// <inheritdoc />
	public Task<List<Link>> GetTransactionLinksAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.LinkUri(transactionId), _context.ListLink, cancellationToken);

	/// <inheritdoc />
	public Task AddLinkToTransactionAsync(Guid transactionId, Guid linkId) =>
		PutAsync(Transactions.LinkIdUri(transactionId, linkId));

	/// <inheritdoc />
	public Task RemoveLinkFromTransactionAsync(Guid transactionId, Guid linkId) =>
		DeleteAsync(Transactions.LinkIdUri(transactionId, linkId));

	/// <inheritdoc />
	public Task<List<Transfer>> GetTransfersAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync(Transfers.TransactionUri(transactionId), _context.ListTransfer, cancellationToken);

	/// <inheritdoc />
	public Task<List<Transfer>> GetTransfersAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Transfers._uri, _context.ListTransfer, cancellationToken);

	/// <inheritdoc />
	public Task<Transfer> GetTransferAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Transfers.IdUri(id), _context.Transfer, cancellationToken);

	/// <inheritdoc />
	public Task PutTransferAsync(Guid id, TransferCreation transfer) =>
		PutAsync(Transfers.IdUri(id), transfer, _context.TransferCreation);

	/// <inheritdoc />
	public Task DeleteTransferAsync(Guid id) =>
		DeleteAsync(Transfers.IdUri(id));

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Purchases._uri, _context.ListPurchase, cancellationToken);

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync(Purchases.TransactionUri(transactionId), _context.ListPurchase, cancellationToken);

	/// <inheritdoc />
	public Task<Purchase> GetPurchaseAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Purchases.IdUri(id), _context.Purchase, cancellationToken);

	/// <inheritdoc />
	public Task PutPurchaseAsync(Guid id, PurchaseCreation purchase) =>
		PutAsync(Purchases.IdUri(id), purchase, _context.PurchaseCreation);

	/// <inheritdoc />
	public Task DeletePurchaseAsync(Guid id) =>
		DeleteAsync(Purchases.IdUri(id));

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Loans._uri, _context.ListLoan, cancellationToken);

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(Guid transactionId, CancellationToken cancellationToken = default) =>
		GetAsync(Loans.TransactionUri(transactionId), _context.ListLoan, cancellationToken);

	/// <inheritdoc />
	public Task<List<Loan>> GetCounterpartyLoansAsync(Guid counterpartyId, CancellationToken cancellationToken = default) =>
		GetAsync(Loans.ForCounterparty(counterpartyId), _context.ListLoan, cancellationToken);

	/// <inheritdoc />
	public Task<Loan> GetLoanAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Loans.IdUri(id), _context.Loan, cancellationToken);

	/// <inheritdoc />
	public Task PutLoanAsync(Guid id, LoanCreation loan) =>
		PutAsync(Loans.IdUri(id), loan, _context.LoanCreation);

	/// <inheritdoc />
	public Task DeleteLoanAsync(Guid id) =>
		DeleteAsync(Loans.IdUri(id));

	/// <inheritdoc />
	public Task<List<Transaction>> GetRelatedTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Transactions.RelatedUri(id), _context.ListTransaction, cancellationToken);

	/// <inheritdoc />
	public Task AddRelatedTransactionAsync(Guid id, Guid relatedId) =>
		PostAsync(Transactions.RelatedUri(id, relatedId));

	/// <inheritdoc />
	public Task RemoveRelatedTransactionAsync(Guid id, Guid relatedId) =>
		DeleteAsync(Transactions.RelatedUri(id, relatedId));

	/// <inheritdoc />
	public Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Accounts.IdUri(id), _context.Account, cancellationToken);

	/// <inheritdoc />
	public Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Accounts._allUri, _context.ListAccount, cancellationToken);

	/// <inheritdoc />
	public Task<Guid> CreateAccountAsync(AccountCreation account) =>
		PostAsync(Accounts._uri, account, _context.AccountCreation);

	/// <inheritdoc />
	public Task PutAccountAsync(Guid id, AccountCreation account) =>
		PutAsync(Accounts.IdUri(id), account, _context.AccountCreation);

	/// <inheritdoc />
	public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreation currency) =>
		PostAsync(Accounts.Currencies(id), currency, _context.AccountInCurrencyCreation);

	/// <inheritdoc />
	public Task RemoveCurrencyFromAccountAsync(Guid id, Guid currencyId) =>
		DeleteAsync(Accounts.CurrencyIdUri(id, currencyId));

	/// <inheritdoc />
	public Task<List<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default) =>
		GetAsync(_currencyUri, _context.ListCurrency, cancellationToken);

	/// <inheritdoc />
	public Task<List<Balance>> GetAccountBalanceAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Accounts.BalanceUri(id), _context.ListBalance, cancellationToken);

	/// <inheritdoc />
	public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Products._uri, _context.ListProduct, cancellationToken);

	/// <inheritdoc />
	public Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Products.IdUri(id), _context.Product, cancellationToken);

	/// <inheritdoc />
	public Task DeleteProductAsync(Guid id) =>
		DeleteAsync(Products.IdUri(id));

	/// <inheritdoc />
	public Task<Unit> GetUnitAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(UnitIdUri(id), _context.Unit, cancellationToken);

	/// <inheritdoc />
	public Task<List<Unit>> GetUnitsAsync(CancellationToken cancellationToken = default) =>
		GetAsync(_unitUri, _context.ListUnit, cancellationToken);

	/// <inheritdoc />
	public Task PutProductAsync(Guid id, ProductCreation product) =>
		PutAsync(Products.IdUri(id), product, _context.ProductCreation);

	/// <inheritdoc />
	public Task PutUnitAsync(Guid id, UnitCreation unit) =>
		PutAsync(UnitIdUri(id), unit, _context.UnitCreation);

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

		return (await importResponse.Content.ReadFromJsonAsync(_context.AccountReportResult))!;
	}

	/// <inheritdoc />
	public Task<List<string>> GetInstitutionsAsync(string countryCode, CancellationToken cancellationToken = default) =>
		GetAsync(Nordigen.Institutions(countryCode), _context.ListString, cancellationToken);

	/// <inheritdoc />
	public async Task<ImportResult> ImportAsync(string id)
	{
		var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault().Id;
		using var importResponse = await _httpClient.PostAsync(Nordigen.Import(id, timeZone), null);
		if (importResponse.StatusCode is HttpStatusCode.OK)
		{
			var results = await importResponse.Content.ReadFromJsonAsync(_context.ListAccountReportResult).ConfigureAwait(false);
			return new SuccessfulImport(results!);
		}

		if (importResponse.StatusCode is HttpStatusCode.Found)
		{
			var redirectUrl = importResponse.Headers.Location!;
			return new NewRequisition(redirectUrl);
		}

		var message = await importResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
		throw new HttpRequestException($"Failed with message: {message}", null, importResponse.StatusCode);
	}

	/// <inheritdoc />
	public Task AddPurchasesFromDocument(Guid transactionId, Guid linkId) =>
		_httpClient.PostAsync(Paperless.Import(transactionId, linkId), null);

	/// <inheritdoc />
	public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Categories._uri, _context.ListCategory, cancellationToken);

	/// <inheritdoc />
	public Task<Category> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Categories.IdUri(id), _context.Category, cancellationToken);

	/// <inheritdoc />
	public Task<Guid> CreateCategoryAsync(CategoryCreation category) =>
		PostAsync(Categories._uri, category, _context.CategoryCreation);

	/// <inheritdoc />
	public Task PutCategoryAsync(Guid id, CategoryCreation category) =>
		PutAsync(Categories.IdUri(id), category, _context.CategoryCreation);

	/// <inheritdoc />
	public Task DeleteCategoryAsync(Guid id) => DeleteAsync(Categories.IdUri(id));

	/// <inheritdoc />
	public Task<List<Purchase>> GetProductPurchasesAsync(Guid id, CancellationToken cancellationToken = default) =>
		GetAsync(Products.PurchasesUri(id), _context.ListPurchase, cancellationToken);

	/// <inheritdoc />
	public Task<List<Access>> GetAccessesAsync(CancellationToken cancellationToken = default) =>
		GetAsync("Access", _context.ListAccess, cancellationToken);

	/// <inheritdoc />
	public Task DeleteOwnerAsync(Guid id) =>
		DeleteAsync(Owners.IdUri(id));

	/// <inheritdoc />
	public Task<List<Ownership>> GetOwnershipsAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Ownerships._uri, _context.ListOwnership, cancellationToken);

	/// <inheritdoc />
	public Task<List<Owner>> GetOwnersAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Owners._uri, _context.ListOwner, cancellationToken);

	/// <inheritdoc />
	public Task PutOwnerAsync(Guid id, OwnerCreation owner) =>
		PutAsync(Owners.IdUri(id), owner, _context.OwnerCreation);

	/// <inheritdoc />
	public Task PutOwnershipAsync(Guid id, OwnershipCreation ownership) =>
		PutAsync(Ownerships.IdUri(id), ownership, _context.OwnershipCreation);

	/// <inheritdoc />
	public Task DeleteOwnershipAsync(Guid id) =>
		DeleteAsync(Ownerships.IdUri(id));

	/// <inheritdoc />
	public Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default) =>
		GetAsync(Users.Uri, _context.ListUser, cancellationToken);

	private static async Task ThrowIfNotSuccessCode(HttpResponseMessage responseMessage)
	{
		if (responseMessage.IsSuccessStatusCode)
		{
			return;
		}

		var message = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
		throw new HttpRequestException($"{responseMessage.StatusCode}; Details: '{message}'", null, responseMessage.StatusCode);
	}

	private async Task<TResult> GetAsync<TResult>(
		string requestUri,
		JsonTypeInfo<TResult> typeInfo,
		CancellationToken cancellationToken)
	{
		using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);

		return (await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken).ConfigureAwait(false))!;
	}

	private async Task<Guid> PostAsync<TRequest>(string requestUri, TRequest request, JsonTypeInfo<TRequest> typeInfo)
	{
		using var response = await _httpClient.PostAsJsonAsync(requestUri, request, typeInfo).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);

		return await response.Content.ReadFromJsonAsync(_context.Guid).ConfigureAwait(false);
	}

	private async Task PostAsync(string requestUri)
	{
		using var response = await _httpClient.PostAsync(requestUri, null);
		await ThrowIfNotSuccessCode(response);
	}

	private async Task PutAsync(string requestUri)
	{
		using var response = await _httpClient.PutAsync(requestUri, null).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);
	}

	private async Task PutAsync<TRequest>(string requestUri, TRequest request, JsonTypeInfo<TRequest> typeInfo)
		where TRequest : notnull
	{
		using var response = await _httpClient.PutAsJsonAsync(requestUri, request, typeInfo).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(response);
	}

	private async Task DeleteAsync(string requestUri)
	{
		var deleteResponse = await _httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
		await ThrowIfNotSuccessCode(deleteResponse);
	}
}
