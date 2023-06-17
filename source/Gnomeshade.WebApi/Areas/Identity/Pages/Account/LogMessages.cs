// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Log messages used by identity pages.</summary>
internal static partial class LogMessages
{
	[LoggerMessage(EventId = 1000, Level = Debug, Message = "Password sign in result {SignInResult}")]
	internal static partial void PasswordSignIn(ILogger logger, SignInResult signInResult);

	[LoggerMessage(EventId = 1001, Level = Information, Message = "User logged in")]
	internal static partial void UserLoggedIn(ILogger logger);

	[LoggerMessage(EventId = 1002, Level = Information, Message = "User with ID '{UserId}' logged in with 2fa")]
	internal static partial void UserLoggedIn2Fa(ILogger logger, string userId);

	[LoggerMessage(EventId = 1003, Level = Information,
		Message = "User with ID '{UserId}' logged in with a recovery code")]
	internal static partial void UserLoggedInRecoveryCode(ILogger logger, string userId);

	[LoggerMessage(EventId = 1004, Level = Information, Message = "{Name} logged in with {LoginProvider} provider")]
	internal static partial void UserLoggedInExternal(ILogger logger, string? name, string loginProvider);

	[LoggerMessage(EventId = 1005, Level = Information, Message = "User created a new account with password")]
	internal static partial void UserCreated(ILogger logger);

	[LoggerMessage(EventId = 1006, Level = Information,
		Message = "User created an account using {LoginProvider} provider")]
	internal static partial void UserCreatedExternal(ILogger logger, string loginProvider);

	[LoggerMessage(EventId = 1007, Level = Warning, Message = "User account locked out")]
	internal static partial void UserLockedOut(ILogger logger);

	[LoggerMessage(EventId = 1008, Level = Warning, Message = "User with ID '{UserId}' account locked out")]
	internal static partial void UserLockedOut(ILogger logger, string userId);

	[LoggerMessage(EventId = 1009, Level = Warning,
		Message = "Invalid authenticator code entered for user with ID '{UserId}'")]
	internal static partial void InvalidAuthenticatorCode(ILogger logger, string userId);

	[LoggerMessage(EventId = 1010, Level = Warning,
		Message = "Invalid recovery code entered for user with ID '{UserId}' ")]
	internal static partial void InvalidRecoveryCode(ILogger logger, string userId);

	[LoggerMessage(EventId = 1011, Level = Debug,
		Message = "User created an account using {LoginProvider} ({ProviderKey}) provider")]
	internal static partial void RetrievedExternalUserInfo(
		this ILogger logger,
		string loginProvider,
		string providerKey);
}
