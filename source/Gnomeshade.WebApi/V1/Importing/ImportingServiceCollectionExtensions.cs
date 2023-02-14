// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.V1.Importing.Paperless;
using Gnomeshade.WebApi.V1.Importing.Paperless.Rimi;

using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.WebApi.V1.Importing;

internal static class ImportingServiceCollectionExtensions
{
	internal static IServiceCollection AddV1ImportingServices(this IServiceCollection services) => services
		.AddTransient<Iso20022AccountReportReader>()
		.AddTransient<IPaperlessDocumentParser, RimiReceiptParser>()
		.AddTransient<IPurchaseIdentifier, RimiPurchaseIdentifier>()
		.AddTransient<IPaperlessService, PaperlessService>();
}
