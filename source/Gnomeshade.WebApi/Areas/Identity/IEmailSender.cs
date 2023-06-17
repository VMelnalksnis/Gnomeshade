// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace Gnomeshade.WebApi.Areas.Identity;

public interface IEmailSender
{
	Task SendEmailAsync(string email, string subject, string htmlMessage);
}
