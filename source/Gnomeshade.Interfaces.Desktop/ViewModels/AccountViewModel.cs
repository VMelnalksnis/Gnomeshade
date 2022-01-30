﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;
using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>
/// Overview of all accounts.
/// </summary>
public sealed class AccountViewModel : ViewModelBase
{
	private AccountOverviewRow? _selectedAccount;

	private AccountViewModel(List<Account> accounts)
	{
		var accountOverviewRows = accounts.Translate().ToList();
		Accounts = new(accountOverviewRows);
		var group = new DataGridTypedGroupDescription<AccountOverviewRow, string>(row => row.Name);
		DataGridView.GroupDescriptions.Add(group);
	}

	/// <summary>
	/// Raised when a account is selected for viewing its details.
	/// </summary>
	public event EventHandler<AccountSelectedEventArgs>? AccountSelected;

	/// <summary>
	/// Gets a grid view of all accounts.
	/// </summary>
	public DataGridCollectionView DataGridView => Accounts;

	/// <summary>
	/// Gets a typed collection of accounts in <see cref="DataGridView"/>.
	/// </summary>
	public DataGridItemCollectionView<AccountOverviewRow> Accounts { get; }

	/// <summary>
	/// Gets or sets the selected row in <see cref="DataGridView"/>.
	/// </summary>
	public AccountOverviewRow? SelectedAccount
	{
		get => _selectedAccount;
		set => SetAndNotify(ref _selectedAccount, value);
	}

	/// <summary>
	/// Asynchronously creates a new instance of the <see cref="AccountViewModel"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of <see cref="AccountViewModel"/>.</returns>
	public static async Task<AccountViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var accounts = await gnomeshadeClient.GetAccountsAsync();
		return new(accounts);
	}

	/// <summary>
	/// Handles the <see cref="DataGrid.DoubleTapped"/> event for <see cref="DataGridView"/>.
	/// </summary>
	public void OnDataGridDoubleTapped()
	{
		if (SelectedAccount is null || AccountSelected is null)
		{
			return;
		}

		AccountSelected(this, new(SelectedAccount.Id));
	}
}
