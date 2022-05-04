// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>A summary of a single transfer for displaying in an overview.</summary>
public sealed class TransferSummary : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransferSummary"/> class.</summary>
	/// <param name="userCurrency">The currency of <paramref name="userAmount"/>.</param>
	/// <param name="displayUserCurrency">Whether <see cref="UserCurrency"/> needs to be displayed.</param>
	/// <param name="userAmount">The amount withdrawn from or deposited to the account owned by the user.</param>
	/// <param name="userAccount">The name of the account owned by the user.</param>
	/// <param name="direction">A symbol indicating whether <see cref="UserAmount"/> was withdrawn from or deposited into <see cref="UserAccount"/>.</param>
	/// <param name="userToUser">Whether the transfer is between user accounts.</param>
	/// <param name="otherCounterparty">The name of the other counterparty.</param>
	/// <param name="otherAccount">The name of the account not owned by the user.</param>
	/// <param name="otherCurrency">The currency of <paramref name="otherAmount"/>.</param>
	/// <param name="otherAmount">The amount withdrawn from or deposited to the account not owned by the user.</param>
	public TransferSummary(
		string userCurrency,
		bool displayUserCurrency,
		decimal userAmount,
		string userAccount,
		string direction,
		bool userToUser,
		string otherCounterparty,
		string otherAccount,
		string otherCurrency,
		decimal otherAmount)
	{
		UserCurrency = userCurrency;
		DisplayUserCurrency = displayUserCurrency;
		UserAmount = userAmount;
		UserAccount = userAccount;
		Direction = direction;
		UserToUser = userToUser;
		OtherCounterparty = otherCounterparty;
		OtherAccount = otherAccount;
		OtherCurrency = otherCurrency;
		OtherAmount = otherAmount;
	}

	/// <summary>Gets the currency of <see cref="UserAmount"/>.</summary>
	public string UserCurrency { get; }

	/// <summary>Gets a value indicating whether <see cref="UserCurrency"/> needs to be displayed.</summary>
	public bool DisplayUserCurrency { get; }

	/// <summary>Gets the amount withdrawn from or deposited to the account owned by the user.</summary>
	public decimal UserAmount { get; }

	/// <summary>Gets the name of the account owned by the user.</summary>
	public string UserAccount { get; }

	/// <summary>Gets a symbol indicating whether <see cref="UserAmount"/> was withdrawn from or deposited into <see cref="UserAccount"/>.</summary>
	public string Direction { get; }

	/// <summary>Gets a value indicating whether the transfer is between user accounts.</summary>
	public bool UserToUser { get; }

	/// <summary>Gets the name of the other counterparty.</summary>
	public string OtherCounterparty { get; }

	/// <summary>Gets the name of the account not owned by the user.</summary>
	public string OtherAccount { get; }

	/// <summary>Gets the name of the other account to display.</summary>
	public string DisplayedAccount => UserToUser ? OtherAccount : OtherCounterparty;

	/// <summary>Gets the currency of <see cref="OtherAmount"/>.</summary>
	public string OtherCurrency { get; }

	/// <summary>Gets the amount withdrawn from or deposited to the account not owned by the user.</summary>
	public decimal OtherAmount { get; }

	/// <summary>Gets a value indicating whether <see cref="OtherAmount"/> and <see cref="OtherCurrency"/> need to be displayed.</summary>
	public bool DisplayTarget => UserAmount != OtherAmount;
}
