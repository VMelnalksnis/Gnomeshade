// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;

/// <summary>Values for filtering transactions.</summary>
public sealed class TransactionFilter : ViewModelBase
{
	private static readonly string[] _isValidNames = { nameof(IsValid) };

	private DateTimeOffset? _fromDate;
	private DateTimeOffset? _toDate;
	private Account? _selectedAccount;
	private List<Account> _accounts = new();
	private List<Counterparty> _counterparties = new();
	private Counterparty? _selectedCounterparty;

	/// <summary>Gets or sets the date from which to get transactions.</summary>
	public DateTimeOffset? FromDate
	{
		get => _fromDate;
		set => SetAndNotifyWithGuard(ref _fromDate, value, nameof(FromDate), _isValidNames);
	}

	/// <summary>Gets or sets the date until which to ge transactions.</summary>
	public DateTimeOffset? ToDate
	{
		get => _toDate;
		set => SetAndNotifyWithGuard(ref _toDate, value, nameof(ToDate), _isValidNames);
	}

	/// <summary>Gets a value indicating whether the current values are valid search parameters.</summary>
	public bool IsValid => ToDate is null || FromDate is null || ToDate >= FromDate;

	/// <summary>Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <summary>Gets a delegate for formatting an counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets or sets a collection of all active accounts.</summary>
	public List<Account> Accounts
	{
		get => _accounts;
		set => SetAndNotify(ref _accounts, value);
	}

	/// <summary>Gets or sets the selected account from <see cref="Accounts"/>.</summary>
	public Account? SelectedAccount
	{
		get => _selectedAccount;
		set => SetAndNotify(ref _selectedAccount, value);
	}

	/// <summary>Gets or sets a collection of all counterparties.</summary>
	public List<Counterparty> Counterparties
	{
		get => _counterparties;
		set => SetAndNotify(ref _counterparties, value);
	}

	/// <summary>Gets or sets the selected counterparty from <see cref="Counterparties"/>.</summary>
	public Counterparty? SelectedCounterparty
	{
		get => _selectedCounterparty;
		set => SetAndNotify(ref _selectedCounterparty, value);
	}

	/// <summary>Predicate for determining if an item is suitable for inclusion in the view.</summary>
	/// <param name="item">The item to check against the filters set in this viewmodel.</param>
	/// <returns><see langword="true"/> if <paramref name="item"/> matches the filters set in this viewmodel; otherwise <see langword="false"/>.</returns>
	public bool Filter(object item)
	{
		if (item is not TransactionOverview overview)
		{
			return false;
		}

		if (SelectedAccount is null && SelectedCounterparty is null)
		{
			return true;
		}

		var matchesAccount = SelectedAccount is null ||
			overview.Transfers.Any(transfer =>
				transfer.UserAccount == SelectedAccount.Name ||
				transfer.OtherAccount == SelectedAccount.Name);

		var matchesCounterparty = SelectedCounterparty is null ||
			overview.Transfers.Any(transfer => transfer.OtherCounterparty == SelectedCounterparty.Name);

		return matchesAccount && matchesCounterparty;
	}
}
