// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Products;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>The act or an instance of buying of a <see cref="Product"/> as a part of a <see cref="Transaction"/>.</summary>
[PublicAPI]
public sealed record PlannedPurchase : PurchaseBase;
