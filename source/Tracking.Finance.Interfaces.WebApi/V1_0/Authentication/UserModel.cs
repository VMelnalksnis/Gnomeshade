﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Authentication
{
	[PublicAPI]
	public sealed record UserModel
	{
		public string Id { get; init; }

		public string UserName { get; init; }

		public string Email { get; init; }
	}
}
