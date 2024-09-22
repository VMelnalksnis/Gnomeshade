// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a planned purchase.</summary>
/// <seealso cref="Purchase"/>
[PublicAPI]
public sealed record PlannedPurchaseCreation : PurchaseCreationBase;
