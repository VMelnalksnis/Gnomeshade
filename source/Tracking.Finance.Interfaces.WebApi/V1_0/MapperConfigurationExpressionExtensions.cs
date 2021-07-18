﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using AutoMapper;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Interfaces.WebApi.V1_0.Accounts;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Products;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.V1_0
{
	/// <summary>
	/// Extensions for configuring AutoMapper for v1.0 API.
	/// </summary>
	public static class MapperConfigurationExpressionExtensions
	{
		/// <summary>
		/// Creates maps for all v1.0 models.
		/// </summary>
		/// <param name="options">The options to which to add the maps.</param>
		public static void CreateMapsForV1_0(this IMapperConfigurationExpression options)
		{
			options.CreateMap<RegistrationModel, ApplicationUser>();
			options.CreateMap<ApplicationUser, UserModel>();

			options.CreateMap<TransactionCreationModel, Transaction>();
			options.CreateMap<Transaction, TransactionModel>();

			options.CreateMap<TransactionItemCreationModel, TransactionItem>();
			options.CreateMap<TransactionItem, TransactionItemModel>();

			options.CreateMap<Account, AccountModel>();
			options.CreateMap<AccountCreationModel, Account>();

			options.CreateMap<AccountInCurrency, AccountInCurrencyModel>();
			options.CreateMap<AccountInCurrencyCreationModel, AccountInCurrency>();

			options.CreateMap<Currency, CurrencyModel>();

			options.CreateMap<Product, ProductModel>();
			options.CreateMap<ProductCreationModel, Product>();

			options.CreateMap<Unit, UnitModel>();
			options.CreateMap<UnitCreationModel, Unit>();
		}
	}
}
