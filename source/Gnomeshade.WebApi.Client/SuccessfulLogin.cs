// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Client;

/// <summary>Indicates a successful login.</summary>
[PublicAPI]
public sealed record SuccessfulLogin : LoginResult;
