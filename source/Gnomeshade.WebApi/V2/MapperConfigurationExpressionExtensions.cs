// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.V2;

/// <summary>Extensions for configuring AutoMapper for v2.0 API.</summary>
public static class MapperConfigurationExpressionExtensions
{
	/// <summary>Creates maps for all v2.0 models.</summary>
	/// <param name="options">The options to which to add the maps.</param>
	public static void CreateMapsForV2_0(this IMapperConfigurationExpression options)
	{
		options.CreateMap<DetailedTransaction2Entity, DetailedTransaction>();

		options.CreateMap<Loan2Entity, Loan>();
		options.CreateMap<LoanCreation, Loan2Entity>();

		options.CreateMap<LoanPaymentEntity, LoanPayment>();
		options.CreateMap<LoanPaymentCreation, LoanPaymentEntity>();
	}
}
