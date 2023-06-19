// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity;

/// <inheritdoc />
public sealed class EmailSender : IEmailSender
{
	private readonly ILogger<EmailSender> _logger;

	public EmailSender(ILogger<EmailSender> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		_logger.WouldSendEmail(subject, email);
		return Task.CompletedTask;
	}
}
