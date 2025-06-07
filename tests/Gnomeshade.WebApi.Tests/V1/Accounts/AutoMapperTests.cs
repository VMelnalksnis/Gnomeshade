// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.V1;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.V1.Accounts;

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
		var creationModel = new AccountCreation
		{
			Name = "Spending",
			CounterpartyId = Guid.NewGuid(),
			PreferredCurrencyId = Guid.NewGuid(),
			Bic = "AAAA-BB-CC-123",
			Iban = "LV97HABA0012345678910",
			AccountNumber = "123456789",
		};

		var account = _mapper.Map<AccountEntity>(creationModel);

		account
			.Should()
			.BeEquivalentTo(
				creationModel,
				options => options
					.ByMembersExcluding<AccountCreation, AccountEntity>(model => model.Currencies)
					.Excluding(model => model.OwnerId));
	}

	[Test]
	public void AccountInCurrencyCreationModel()
	{
		var creationModel = new AccountInCurrencyCreation
		{
			CurrencyId = Guid.NewGuid(),
		};

		var accountInCurrency = _mapper.Map<AccountInCurrencyEntity>(creationModel);

		accountInCurrency
			.Should()
			.BeEquivalentTo(
				creationModel,
				options => options
					.ByMembers<AccountInCurrencyCreation, AccountInCurrencyEntity>()
					.Excluding(model => model.OwnerId));
	}

	[Test]
	public void AccountInCurrency()
	{
		var accountInCurrency = new AccountInCurrencyEntity
		{
			Id = Guid.NewGuid(),
			CreatedAt = SystemClock.Instance.GetCurrentInstant(),
			OwnerId = Guid.NewGuid(),
			CreatedByUserId = Guid.NewGuid(),
			ModifiedAt = SystemClock.Instance.GetCurrentInstant(),
			ModifiedByUserId = Guid.NewGuid(),
		};

		var accountInCurrencyModel = _mapper.Map<AccountInCurrency>(accountInCurrency);

		accountInCurrencyModel
			.Should()
			.BeEquivalentTo(
				accountInCurrency,
				options => options
					.ByMembersExcluding<AccountInCurrencyEntity, AccountInCurrency>(
						inCurrency => inCurrency.CurrencyId,
						inCurrency => inCurrency.AccountId,
						inCurrency => inCurrency.DeletedAt,
						inCurrency => inCurrency.DeletedByUserId));
	}

	[Test]
	public void AccountModel()
	{
		var account = new AccountEntity
		{
			Id = Guid.NewGuid(),
			CreatedAt = SystemClock.Instance.GetCurrentInstant(),
			OwnerId = Guid.NewGuid(),
			CreatedByUserId = Guid.NewGuid(),
			ModifiedAt = SystemClock.Instance.GetCurrentInstant(),
			ModifiedByUserId = Guid.NewGuid(),
			Name = "Spending",
			NormalizedName = "SPENDING",
			PreferredCurrencyId = Guid.NewGuid(),
			Bic = "AAAA-BB-CC-123",
			Iban = "LV97HABA0012345678910",
			AccountNumber = "123456789",
			Currencies =
			[
				new()
				{
					Id = Guid.NewGuid(),
					CreatedAt = SystemClock.Instance.GetCurrentInstant(),
					OwnerId = Guid.NewGuid(),
					CreatedByUserId = Guid.NewGuid(),
					ModifiedAt = SystemClock.Instance.GetCurrentInstant(),
					ModifiedByUserId = Guid.NewGuid(),
				},
			],
		};

		var accountModel = _mapper.Map<Account>(account);

		accountModel
			.Should()
			.BeEquivalentTo(
				account,
				options => options
					.ByMembersExcluding<AccountEntity, Account>(
						a => a.NormalizedName,
						a => a.PreferredCurrencyId,
						a => a.Currencies,
						a => a.DeletedAt,
						a => a.DeletedByUserId));

		accountModel
			.Currencies
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeEquivalentTo(
				account.Currencies.Single(),
				options => options
					.ByMembersExcluding<AccountInCurrencyEntity, AccountInCurrency>(
						inCurrency => inCurrency.CurrencyId,
						inCurrency => inCurrency.AccountId,
						inCurrency => inCurrency.DeletedAt,
						inCurrency => inCurrency.DeletedByUserId));
	}

	[Test]
	public void CurrencyModel()
	{
		var currency = new CurrencyEntity
		{
			Id = Guid.NewGuid(),
			CreatedAt = SystemClock.Instance.GetCurrentInstant(),
			Name = "Euro",
			NormalizedName = "EURO",
			NumericCode = 987,
			AlphabeticCode = "EUR",
			MinorUnit = 2,
			Official = true,
			Crypto = false,
			Historical = false,
			ActiveFrom = Instant.FromUtc(1990, 01, 01, 0, 0, 0),
			ActiveUntil = null,
		};

		var currencyModel = _mapper.Map<Currency>(currency);

		currencyModel
			.Should()
			.BeEquivalentTo(
				currency,
				options => options.ByMembersExcluding<CurrencyEntity, Currency>(
					c => c.NormalizedName,
					c => c.DeletedAt,
					c => c.DeletedByUserId,
					c => c.CreatedByUserId));
	}
}
