// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Loans;

/// <summary>A payment that was to either issue or pay back a loan.</summary>
/// <seealso cref="LoanPaymentCreation"/>
[PublicAPI]
public sealed record LoanPayment : LoanPaymentBase;
