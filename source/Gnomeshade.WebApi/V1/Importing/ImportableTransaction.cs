// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using NodaTime;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

namespace Gnomeshade.WebApi.V1.Importing;

internal sealed record ImportableTransaction(
	string? BankReference,
	string? ExternalReference,
	decimal Amount,
	string CurrencyCode,
	CreditDebitCode CreditDebitCode,
	Instant BookingDate,
	Instant? ValueDate,
	string? Description,
	string OtherCurrencyCode,
	decimal OtherAmount,
	string? OtherAccountIban,
	string? OtherAccountName,
	string? DomainCode,
	string? FamilyCode,
	string? SubFamilyCode);
