// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Owners;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Accesses;

/// <summary>A row representing an <see cref="Ownership"/>.</summary>
public sealed partial class OwnershipRow : PropertyChangedBase
{
	/// <summary>Gets the access level of the ownership.</summary>
	[Notify(Setter.Private)]
	private string _access;

	/// <summary>Gets the name of the user which was given the <see cref="Access"/>.</summary>
	[Notify(Setter.Private)]
	private string _username;

	/// <summary>Initializes a new instance of the <see cref="OwnershipRow"/> class.</summary>
	/// <param name="access">The access level.</param>
	/// <param name="counterparty">The user which was given <paramref name="access"/>.</param>
	public OwnershipRow(Access access, Counterparty counterparty)
	{
		_access = access.Name;
		AccessId = access.Id;
		_username = counterparty.Name;
		UserId = counterparty.Id;
	}

	internal Guid? Id { get; set; }

	internal Guid UserId { get; private set; }

	internal Guid AccessId { get; private set; }

	internal void SetUser(Counterparty counterparty)
	{
		Username = counterparty.Name;
		UserId = counterparty.Id;
	}

	internal void SetAccess(Access access)
	{
		Access = access.Name;
		AccessId = access.Id;
	}
}
