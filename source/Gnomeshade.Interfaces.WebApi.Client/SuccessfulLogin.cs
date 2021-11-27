// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.Models.Authentication;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>
/// Indicates a successful login.
/// </summary>
/// <param name="Response">The information return after successful login.</param>
public sealed record SuccessfulLogin(LoginResponse Response) : LoginResult;
