// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.Tests.Integration;

internal static class FluentAssertionsExtensions
{
	internal static EquivalencyAssertionOptions<Transaction> WithoutModifiedAt(
		this EquivalencyAssertionOptions<Transaction> options) => options
		.ComparingByMembers<Transaction>()
		.Excluding(transaction => transaction.ModifiedAt);

	internal static EquivalencyAssertionOptions<Transfer> WithoutTransaction(
		this EquivalencyAssertionOptions<Transfer> options) => options
		.ComparingByMembers<Transfer>()
		.Excluding(transfer => transfer.ModifiedAt)
		.Excluding(transfer => transfer.TransactionId);

	internal static EquivalencyAssertionOptions<Purchase> WithoutTransaction(
		this EquivalencyAssertionOptions<Purchase> options) => options
		.ComparingByMembers<Purchase>()
		.Excluding(purchase => purchase.ModifiedAt)
		.Excluding(purchase => purchase.TransactionId);

	internal static EquivalencyAssertionOptions<Loan> WithoutTransaction(
		this EquivalencyAssertionOptions<Loan> options) => options
		.ComparingByMembers<Loan>()
		.Excluding(loan => loan.ModifiedAt)
		.Excluding(loan => loan.TransactionId);

	internal static EquivalencyAssertionOptions<Link> WithoutModifiedAt(
		this EquivalencyAssertionOptions<Link> options) => options
		.ComparingByMembers<Link>()
		.Excluding(loan => loan.ModifiedAt);
}
