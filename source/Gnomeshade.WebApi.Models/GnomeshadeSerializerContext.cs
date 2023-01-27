// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Authentication;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.Models;

/// <inheritdoc cref="System.Text.Json.Serialization.JsonSerializerContext" />
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<Link>))]
[JsonSerializable(typeof(LinkCreation))]
[JsonSerializable(typeof(List<DetailedTransaction>))]
[JsonSerializable(typeof(List<Transaction>))]
[JsonSerializable(typeof(TransactionCreation))]
[JsonSerializable(typeof(List<Loan>))]
[JsonSerializable(typeof(LoanCreation))]
[JsonSerializable(typeof(List<Purchase>))]
[JsonSerializable(typeof(PurchaseCreation))]
[JsonSerializable(typeof(List<Transfer>))]
[JsonSerializable(typeof(TransferCreation))]
[JsonSerializable(typeof(List<Account>))]
[JsonSerializable(typeof(AccountCreation))]
[JsonSerializable(typeof(List<AccountInCurrency>))]
[JsonSerializable(typeof(AccountInCurrencyCreation))]
[JsonSerializable(typeof(List<Balance>))]
[JsonSerializable(typeof(List<Counterparty>))]
[JsonSerializable(typeof(CounterpartyCreation))]
[JsonSerializable(typeof(List<Currency>))]
[JsonSerializable(typeof(List<AccountReportResult>))]
[JsonSerializable(typeof(List<Access>))]
[JsonSerializable(typeof(List<Owner>))]
[JsonSerializable(typeof(List<Ownership>))]
[JsonSerializable(typeof(OwnershipCreation))]
[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(CategoryCreation))]
[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(ProductCreation))]
[JsonSerializable(typeof(List<Unit>))]
[JsonSerializable(typeof(UnitCreation))]
[JsonSerializable(typeof(Login))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(RegistrationModel))]
[JsonSerializable(typeof(UserModel))]
internal sealed partial class GnomeshadeSerializerContext : JsonSerializerContext
{
}
