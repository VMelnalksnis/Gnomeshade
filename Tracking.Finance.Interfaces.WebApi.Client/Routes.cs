// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	internal static class Routes
	{
		internal static readonly string Authentication = typeof(AuthenticationController).GetControllerName();
		internal static readonly string Transaction = typeof(TransactionController).GetControllerName();

		internal static readonly string LoginUri = $"{Authentication}/{nameof(AuthenticationController.Login)}";
		internal static readonly string InfoUri = $"{Authentication}/{nameof(AuthenticationController.Info)}";

		internal static string TransactionItemUri(int transactionId) => $"{Transaction}/{transactionId}/Item";
	}
}
