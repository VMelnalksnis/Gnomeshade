﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Tracking.Finance.Interfaces.WebApi.Client.Login;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
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
		public async Task<LoginResult> Login(LoginModel login)
		{
			try
			{
				using var response = await _httpClient.PostAsJsonAsync(LoginUri, login);
				if (response.IsSuccessStatusCode)
				{
					var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
					_httpClient.DefaultRequestHeaders.Authorization = new(JwtBearerDefaults.AuthenticationScheme, loginResponse.Token);
					return new SuccessfulLogin(loginResponse);
				}

				var errorResponse = await response.Content.ReadAsStringAsync();
				return new FailedLogin(response.StatusCode, errorResponse);
			}
			catch (HttpRequestException httpException)
			{
				return new FailedLogin(httpException.StatusCode, httpException.Message);
			}
		}

		/// <inheritdoc/>
		public Task<UserModel> Info() => Get<UserModel>(InfoUri);

		/// <inheritdoc/>
		public Task<Guid> Create(TransactionCreationModel transaction)
		{
			return Post<Guid, TransactionCreationModel>(Transaction, transaction);
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> Get()
		{
			return Get<List<TransactionModel>>(Transaction);
		}

		private async Task<TResult> Get<TResult>(string requestUri)
		{
			using var response = await _httpClient.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<TResult>())!;
		}

		private async Task<TResult> Post<TResult, TRequest>(string requestUri, TRequest request)
		{
			using var response = await _httpClient.PostAsJsonAsync(requestUri, request);
			response.EnsureSuccessStatusCode();

			return (await response.Content.ReadFromJsonAsync<TResult>())!;
		}
	}
}
