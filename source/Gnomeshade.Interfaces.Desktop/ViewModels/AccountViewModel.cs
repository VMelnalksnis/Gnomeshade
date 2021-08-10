// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// Overview of all accounts.
	/// </summary>
	public sealed class AccountViewModel : ViewModelBase<AccountView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private DataGridItemCollectionView<AccountOverviewRow> _accounts = null!;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountViewModel"/> class.
		/// Should only be used during design time.
		/// </summary>
		public AccountViewModel()
			: this(new DesignTimeGnomeshadeClient())
		{
			GetAccountsAsync().GetAwaiter().GetResult();
		}

		private AccountViewModel(IGnomeshadeClient gnomeshadeClient)
		{
			_gnomeshadeClient = gnomeshadeClient;
		}

		/// <summary>
		/// Gets a grid view of all accounts.
		/// </summary>
		public DataGridCollectionView DataGridView => Accounts;

		/// <summary>
		/// Gets a typed collection of accounts in <see cref="DataGridView"/>.
		/// </summary>
		public DataGridItemCollectionView<AccountOverviewRow> Accounts
		{
			get => _accounts;
			private set => SetAndNotifyWithGuard(ref _accounts, value, nameof(Accounts), nameof(DataGridView));
		}

		/// <summary>
		/// Asynchronously initializes a new instance of the <see cref="AccountViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">API client for getting finance data.</param>
		/// <returns>A new instance of <see cref="AccountViewModel"/>.</returns>
		public static async Task<AccountViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
		{
			var viewModel = new AccountViewModel(gnomeshadeClient);
			await viewModel.GetAccountsAsync().ConfigureAwait(false);
			return viewModel;
		}

		private async Task GetAccountsAsync()
		{
			var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
			var accountOverviewRows = accounts.Translate().ToList();
			Accounts = new(accountOverviewRows);
			var group = new DataGridTypedGroupDescription<AccountOverviewRow, string>(row => row.Name);
			DataGridView.GroupDescriptions.Add(group);
		}
	}
}
