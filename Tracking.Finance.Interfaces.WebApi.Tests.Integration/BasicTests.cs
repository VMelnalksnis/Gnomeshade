// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.Configuration;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Tests.Integration
{
	public class BasicTests
	{
		private WebApplicationFactory<Startup> _applicationFactory = null!;
		private AuthenticatedUserOptions _userOptions = null!;

		private HttpClient _client = null!;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_applicationFactory = new();

			var builder = new ConfigurationBuilder()
				.AddUserSecrets<BasicTests>()
				.AddEnvironmentVariables();

			var configuration = builder.Build();
			_userOptions = configuration.GetValid<AuthenticatedUserOptions>();
		}

		[SetUp]
		public void SetUp()
		{
			_client = _applicationFactory.CreateClient();
		}

		[TestCase("v1.0")]
		public async Task Get_SwaggerEndpoint(string version)
		{
			var response = await _client.GetAsync($"/swagger/{version}/swagger.json");

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[TestCase("/swagger")]
		public async Task Get_ReturnsSuccess(string requestUrl)
		{
			var response = await _client.GetAsync(requestUrl);

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[Test]
		public async Task Get_ShouldReturnUnauthorized()
		{
			var response = await _client.GetAsync("/api/v1.0/transaction");

			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}

		[Test]
		public async Task Login()
		{
			await Authorize();

			var authorizedResponse = await _client.GetAsync("/api/v1.0/transaction");
			authorizedResponse.EnsureSuccessStatusCode();
		}

		[Test]
		public async Task Create_ShouldCreateItems()
		{
			await Authorize();
			var transactionsResponse = await _client.GetAsync("/api/v1.0/transaction");
			var transactions = await transactionsResponse.Content.ReadFromJsonAsync<List<TransactionModel>>();

			var transaction = new TransactionCreationModel
			{
				Date = DateTimeOffset.Now,
				Description = "Transaction with items creation test",
				Items = new List<TransactionItemCreationModel>
				{
					new TransactionItemCreationModel
					{
						SourceAccountId = 0,
						TargetAccountId = 0,
						SourceAmount = 0,
						TargetAmount = 0,
						ProductId = 0,
						Amount = 0,
					},
				},
			};

			var createResponse = await _client.PostAsJsonAsync("/api/v1.0/transaction", transaction);
			createResponse.EnsureSuccessStatusCode();
		}

		private async Task Authorize()
		{
			var login = new LoginModel { Username = _userOptions.Username, Password = _userOptions.Password };

			var loginResponse = await _client.PostAsJsonAsync("/api/v1.0/authentication/login", login);
			loginResponse.EnsureSuccessStatusCode();
			var responseContent = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!;

			var header = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, responseContent.Token);
			_client.DefaultRequestHeaders.Authorization = header;
		}
	}
}
