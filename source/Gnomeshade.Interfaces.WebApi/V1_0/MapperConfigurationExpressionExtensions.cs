// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Tags;
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

		options.CreateMap<TransactionCreationModel, TransactionEntity>();
		options.CreateMap<TransactionEntity, Transaction>();

		options.CreateMap<TransactionItemCreationModel, TransactionItemEntity>();
		options.CreateMap<TransactionItemEntity, TransactionItem>();

		options.CreateMap<AccountEntity, Account>();
		options.CreateMap<AccountCreationModel, AccountEntity>();

		options.CreateMap<AccountInCurrencyEntity, AccountInCurrency>();
		options.CreateMap<AccountInCurrencyCreationModel, AccountInCurrencyEntity>();

		options.CreateMap<CounterpartyEntity, Counterparty>();
		options.CreateMap<CounterpartyCreationModel, CounterpartyEntity>();

		options.CreateMap<CurrencyEntity, Currency>();

		options.CreateMap<ProductEntity, Product>();
		options.CreateMap<ProductCreationModel, ProductEntity>();

		options.CreateMap<TagEntity, Tag>();
		options.CreateMap<TagCreation, TagEntity>();

		options.CreateMap<UnitEntity, Unit>();
		options.CreateMap<UnitCreationModel, UnitEntity>();

		options.CreateMap<DateTimeOffset, DateTimeOffset>().ConvertUsing<DateTimeOffsetUtcConverter>();
		options.CreateMap<DateTimeOffset?, DateTimeOffset?>().ConvertUsing<DateTimeOffsetUtcConverter>();
	}
}
