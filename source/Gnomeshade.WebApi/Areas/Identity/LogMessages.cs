// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.WebApi.Areas.Identity;

/// <summary>Log messages used by identity pages.</summary>
internal static partial class LogMessages
{
	[LoggerMessage(EventId = 1000, Level = Debug, Message = "Password sign in result {SignInResult}")]
	internal static partial void PasswordSignIn(this ILogger logger, SignInResult signInResult);

	[LoggerMessage(EventId = 1001, Level = Information, Message = "User logged in")]
	internal static partial void UserLoggedIn(this ILogger logger);

	[LoggerMessage(EventId = 1002, Level = Information, Message = "User with ID '{UserId}' logged in with 2fa")]
	internal static partial void UserLoggedIn2Fa(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1003, Level = Information, Message = "User with ID '{UserId}' logged in with a recovery code")]
	internal static partial void UserLoggedInRecoveryCode(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1004, Level = Information, Message = "{Name} logged in with {LoginProvider} provider")]
	internal static partial void UserLoggedInExternal(this ILogger logger, string? name, string loginProvider);

	[LoggerMessage(EventId = 1005, Level = Information, Message = "User created a new account with password")]
	internal static partial void UserCreated(this ILogger logger);

	[LoggerMessage(EventId = 1006, Level = Information, Message = "User created an account using {LoginProvider} provider")]
	internal static partial void UserCreatedExternal(this ILogger logger, string loginProvider);

	[LoggerMessage(EventId = 1007, Level = Warning, Message = "User account locked out")]
	internal static partial void UserLockedOut(this ILogger logger);

	[LoggerMessage(EventId = 1008, Level = Warning, Message = "User with ID '{UserId}' account locked out")]
	internal static partial void UserLockedOut(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1009, Level = Warning, Message = "Invalid authenticator code entered for user with ID '{UserId}'")]
	internal static partial void InvalidAuthenticatorCode(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1010, Level = Warning, Message = "Invalid recovery code entered for user with ID '{UserId}' ")]
	internal static partial void InvalidRecoveryCode(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1011, Level = Debug, Message = "User created an account using {LoginProvider} ({ProviderKey}) provider")]
	internal static partial void RetrievedExternalUserInfo(this ILogger logger, string loginProvider, string providerKey);

	[LoggerMessage(EventId = 1012, Level = Information, Message = "Would send email {EmailSubject} to {EmailRecipient}")]
	internal static partial void WouldSendEmail(this ILogger logger, string emailSubject, string emailRecipient);

	[LoggerMessage(EventId = 1013, Level = Information, Message = "User logged out")]
	internal static partial void UserLoggedOut(this ILogger logger);

	[LoggerMessage(EventId = 1014, Level = Information, Message = "User changed their password successfully")]
	internal static partial void UserChangedPassword(this ILogger logger);

	[LoggerMessage(EventId = 1015, Level = Information, Message = "User with ID '{UserId}' deleted themselves")]
	internal static partial void UserDeletedThemselves(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1016, Level = Information, Message = "User with ID '{UserId}' has disabled 2fa")]
	internal static partial void UserDisabled2Fa(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1017, Level = Information, Message = "User with ID '{UserId}' asked for their personal data")]
	internal static partial void UserRequestedPersonalData(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1018, Level = Information, Message = "User with ID '{UserId}' has enabled 2FA with an authenticator app")]
	internal static partial void UserEnabled2Fa(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1019, Level = Information, Message = "User with ID '{UserId}' has generated new 2FA recovery codes")]
	internal static partial void UserGenerated2FaCodes(this ILogger logger, Guid userId);

	[LoggerMessage(EventId = 1020, Level = Information, Message = "User with ID '{UserId}' has reset their authentication app key")]
	internal static partial void UserReset2Fa(this ILogger logger, Guid userId);
}
