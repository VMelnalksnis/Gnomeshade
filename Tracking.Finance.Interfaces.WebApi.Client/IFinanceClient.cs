// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the API provided by the Interfaces.WebApi project.
	/// </summary>
	public interface IFinanceClient
	{
		/// <summary>
		/// Log in using the specified credentials.
		/// </summary>
		///
		/// <param name="login">The credentials to use to log in.</param>
		/// <returns>Information about the created user session.</returns>
		Task<LoginResponse> Login(LoginModel login);

		/// <summary>
		/// Gets information about the currently logged in user.
		/// </summary>
		///
		/// <returns>Information about the currently logged in user.</returns>
		Task<UserModel> Info();

		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		///
		/// <param name="transaction">Information for creating the transaction.</param>
		/// <returns>The id of the created transaction.</returns>
		Task<int> Create(TransactionCreationModel transaction);

		/// <summary>
		/// Creates a new transaction item.
		/// </summary>
		///
		/// <param name="transactionId">The id of the transaction to which to add the item to.</param>
		/// <param name="transactionItem">Information for creating the transaction item.</param>
		/// <returns>The id of the created transaction item.</returns>
		Task<int> CreateItem(int transactionId, TransactionItemCreationModel transactionItem);
	}
}
