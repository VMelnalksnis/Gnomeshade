// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;
using Gnomeshade.WebApi.Tests.V1.Importing.Paperless.Rimi;
using Gnomeshade.WebApi.V1.Importing.Paperless;

namespace Gnomeshade.WebApi.Tests.V1.Importing.Paperless;

public sealed class PurchaseIdentifierTests
{
	private static readonly Guid _kilogramId = Guid.NewGuid();
	private static readonly Guid _gramId = Guid.NewGuid();

	private readonly CurrencyEntity[] _currencies =
	{
		new() { AlphabeticCode = "EUR" },
	};

	private readonly UnitEntity[] _units =
	{
		new() { Name = "Gram", Symbol = "g", Id = _gramId },
		new() { Name = "Kilogram", Symbol = "kg", Id = _kilogramId },
	};

	internal static ProductEntity[] Products { get; } =
	{
		new() { Name = "Tostermaize franču Brioche", UnitId = _gramId },
	};

	[TestCaseSource(typeof(RimiPurchaseIdentifierTestCaseSource))]
	public void IdentifyPurchase_ShouldReturnExpected(
		IPurchaseIdentifier identifier,
		string purchaseText,
		IdentifiedPurchase expectedPurchase)
	{
		var purchase = identifier.IdentifyPurchase(purchaseText, Products, _currencies, _units);

		purchase.Should().BeEquivalentTo(expectedPurchase);
	}
}
