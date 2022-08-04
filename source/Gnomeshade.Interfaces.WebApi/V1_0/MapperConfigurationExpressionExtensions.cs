// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Owners;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

/// <summary>Extensions for configuring AutoMapper for v1.0 API.</summary>
public static class MapperConfigurationExpressionExtensions
{
	/// <summary>Creates maps for all v1.0 models.</summary>
	/// <param name="options">The options to which to add the maps.</param>
	public static void CreateMapsForV1_0(this IMapperConfigurationExpression options)
	{
		options.AllowNullCollections = true;

		options.CreateMap<RegistrationModel, ApplicationUser>();
		options.CreateMap<ApplicationUser, UserModel>();

		options.CreateMap<TransactionCreation, TransactionEntity>();
		options.CreateMap<TransactionEntity, Transaction>();
		options.CreateMap<TransactionEntity, DetailedTransaction>();

		options.CreateMap<PurchaseCreation, PurchaseEntity>();
		options.CreateMap<PurchaseEntity, Purchase>();

		options.CreateMap<TransferCreation, TransferEntity>();
		options.CreateMap<TransferEntity, Transfer>();

		options.CreateMap<PendingTransferCreation, PendingTransferEntity>();
		options.CreateMap<PendingTransferEntity, PendingTransfer>();

		options.CreateMap<AccountEntity, Account>();
		options.CreateMap<AccountCreation, AccountEntity>();

		options.CreateMap<AccountInCurrencyEntity, AccountInCurrency>();
		options.CreateMap<AccountInCurrencyCreation, AccountInCurrencyEntity>();

		options.CreateMap<CounterpartyEntity, Counterparty>();
		options.CreateMap<CounterpartyCreation, CounterpartyEntity>();

		options.CreateMap<CurrencyEntity, Currency>();
		options.CreateMap<Currency, CurrencyEntity>();

		options.CreateMap<ProductEntity, Product>();
		options.CreateMap<ProductCreation, ProductEntity>();

		options.CreateMap<CategoryEntity, Category>();
		options.CreateMap<CategoryCreation, CategoryEntity>();

		options.CreateMap<UnitEntity, Unit>();
		options.CreateMap<UnitCreation, UnitEntity>();

		options.CreateMap<LinkEntity, Link>();
		options.CreateMap<LinkCreation, LinkEntity>();

		options.CreateMap<LoanEntity, Loan>();
		options.CreateMap<LoanCreation, LoanEntity>();

		options.CreateMap<BalanceEntity, Balance>();

		options.CreateMap<AccessEntity, Access>();

		options.CreateMap<OwnershipEntity, Ownership>();
		options.CreateMap<OwnershipCreation, OwnershipEntity>();

		options.CreateMap<OwnerEntity, Owner>();
	}
}
