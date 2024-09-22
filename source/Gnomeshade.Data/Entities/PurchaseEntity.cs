// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents the purchasing of a product or a service.</summary>
public sealed record PurchaseEntity : PurchaseBase
{
	/// <summary>Gets or sets the date when the <see cref="PurchaseBase.ProductId"/> was delivered.</summary>
	public Instant? DeliveryDate { get; set; }
}
