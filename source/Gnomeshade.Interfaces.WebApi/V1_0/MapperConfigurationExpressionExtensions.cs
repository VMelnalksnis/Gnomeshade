// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using AutoMapper;

using Gnomeshade.Data.Identity;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.WebApi.V1_0
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
			options.AllowNullCollections = true;

			options.CreateMap<RegistrationModel, ApplicationUser>();
			options.CreateMap<ApplicationUser, UserModel>();

			options.CreateMap<TransactionCreationModel, Data.Models.Transaction>();
			options.CreateMap<Data.Models.Transaction, TransactionModel>();

			options.CreateMap<TransactionItemCreationModel, Data.Models.TransactionItem>();
			options.CreateMap<Data.Models.TransactionItem, TransactionItemModel>();

			options.CreateMap<Data.Models.Account, Account>();
			options.CreateMap<AccountCreationModel, Data.Models.Account>();

			options.CreateMap<Data.Models.AccountInCurrency, AccountInCurrency>();
			options.CreateMap<AccountInCurrencyCreationModel, Data.Models.AccountInCurrency>();

			options.CreateMap<Data.Models.Counterparty, Counterparty>();
			options.CreateMap<CounterpartyCreationModel, Data.Models.Counterparty>();

			options.CreateMap<Data.Models.Currency, CurrencyModel>();

			options.CreateMap<Data.Models.Product, ProductModel>();
			options.CreateMap<ProductCreationModel, Data.Models.Product>();

			options.CreateMap<Data.Models.Unit, UnitModel>();
			options.CreateMap<UnitCreationModel, Data.Models.Unit>();
		}
	}
}
