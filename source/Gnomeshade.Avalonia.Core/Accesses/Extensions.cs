// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Owners;

namespace Gnomeshade.Avalonia.Core.Accesses;

internal static class Extensions
{
	internal static IEnumerable<OwnershipRow> GetRows(
		this IEnumerable<Ownership> ownerships,
		IEnumerable<Access> accesses,
		IEnumerable<User> users,
		IEnumerable<Counterparty> counterparties) => ownerships
		.Where(ownership => counterparties.Any(counterparty => counterparty.Id == ownership.UserId))
		.Select(ownership =>
		{
			var access = accesses.Single(access => access.Id == ownership.AccessId);
			var user = users.Single(user => user.Id == ownership.UserId);
			var counterparty = counterparties.Single(counterparty => counterparty.Id == user.Id);
			return new OwnershipRow(access, counterparty) { Id = ownership.Id };
		});
}
