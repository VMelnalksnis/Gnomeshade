// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;

using AutoMapper;

using FluentAssertions;

using Gnomeshade.Data.Models;
using Gnomeshade.Interfaces.WebApi.V1_0;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.V1_0.Accounts
{
	public class AutoMapperTests
	{
		private IMapper _mapper = null!;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_mapper = new MapperConfiguration(options => options.CreateMapsForV1_0()).CreateMapper();
		}

		[Test]
		public void AccountCreationModel()
		{
			var creationModel = new AccountCreationModel
			{
				Name = "Spending",
				PreferredCurrencyId = Guid.NewGuid(),
				Bic = "AAAA-BB-CC-123",
				Iban = "LV97HABA0012345678910",
				AccountNumber = "123456789",
			};

			var account = _mapper.Map<Account>(creationModel);

			account
				.Should()
				.BeEquivalentTo(
					creationModel,
					options => options.ByMembersExcluding<AccountCreationModel, Account>(model => model.Currencies));
		}

		[Test]
		public void AccountInCurrencyCreationModel()
		{
			var creationModel = new AccountInCurrencyCreationModel
			{
				CurrencyId = Guid.NewGuid(),
			};

			var accountInCurrency = _mapper.Map<AccountInCurrency>(creationModel);

			accountInCurrency
				.Should()
				.BeEquivalentTo(
					creationModel,
					options => options.ByMembers<AccountInCurrencyCreationModel, AccountInCurrency>());
		}

		[Test]
		public void AccountInCurrency()
		{
			var accountInCurrency = new AccountInCurrency
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTimeOffset.Now,
				OwnerId = Guid.NewGuid(),
				CreatedByUserId = Guid.NewGuid(),
				ModifiedAt = DateTimeOffset.Now,
				ModifiedByUserId = Guid.NewGuid(),
			};

			var accountInCurrencyModel = _mapper.Map<AccountInCurrencyModel>(accountInCurrency);

			accountInCurrencyModel
				.Should()
				.BeEquivalentTo(
					accountInCurrency,
					options => options
						.ByMembersExcluding<AccountInCurrency, AccountInCurrencyModel>(
							inCurrency => inCurrency.CurrencyId,
							inCurrency => inCurrency.AccountId));
		}

		[Test]
		public void AccountModel()
		{
			var account = new Account
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTimeOffset.Now,
				OwnerId = Guid.NewGuid(),
				CreatedByUserId = Guid.NewGuid(),
				ModifiedAt = DateTimeOffset.Now,
				ModifiedByUserId = Guid.NewGuid(),
				Name = "Spending",
				NormalizedName = "SPENDING",
				PreferredCurrencyId = Guid.NewGuid(),
				Bic = "AAAA-BB-CC-123",
				Iban = "LV97HABA0012345678910",
				AccountNumber = "123456789",
				Currencies = new()
				{
					new()
					{
						Id = Guid.NewGuid(),
						CreatedAt = DateTimeOffset.Now,
						OwnerId = Guid.NewGuid(),
						CreatedByUserId = Guid.NewGuid(),
						ModifiedAt = DateTimeOffset.Now,
						ModifiedByUserId = Guid.NewGuid(),
					},
				},
			};

			var accountModel = _mapper.Map<AccountModel>(account);

			accountModel
				.Should()
				.BeEquivalentTo(
					account,
					options => options
						.ByMembersExcluding<Account, AccountModel>(
							a => a.NormalizedName,
							a => a.PreferredCurrencyId,
							a => a.Currencies));

			accountModel
				.Currencies
				.Should()
				.ContainSingle()
				.Which
				.Should()
				.BeEquivalentTo(
					account.Currencies.Single(),
					options => options
						.ByMembersExcluding<AccountInCurrency, AccountInCurrencyModel>(
							inCurrency => inCurrency.CurrencyId,
							inCurrency => inCurrency.AccountId));
		}

		[Test]
		public void CurrencyModel()
		{
			var currency = new Currency
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTimeOffset.Now,
				Name = "Euro",
				NormalizedName = "EURO",
				NumericCode = 987,
				AlphabeticCode = "EUR",
				MinorUnit = 2,
				Official = true,
				Crypto = false,
				Historical = false,
				ActiveFrom = new DateTimeOffset(1990, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ActiveUntil = null,
			};

			var currencyModel = _mapper.Map<CurrencyModel>(currency);

			currencyModel
				.Should()
				.BeEquivalentTo(
					currency,
					options => options.ByMembersExcluding<Currency, CurrencyModel>(c => c.NormalizedName));
		}
	}
}
