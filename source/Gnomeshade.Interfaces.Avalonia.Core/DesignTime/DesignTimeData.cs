// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.Authentication;
using Gnomeshade.Interfaces.Avalonia.Core.Counterparties;
using Gnomeshade.Interfaces.Avalonia.Core.Imports;
using Gnomeshade.Interfaces.Avalonia.Core.Products;
using Gnomeshade.Interfaces.Avalonia.Core.Tags;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

using IdentityModel.OidcClient;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <summary>Data needed only during design time.</summary>
public static class DesignTimeData
{
	private static IDateTimeZoneProvider DateTimeZoneProvider => DateTimeZoneProviders.Tzdb;

	private static DesignTimeGnomeshadeClient GnomeshadeClient { get; } = new();

	private static OidcClient OidcClient { get; } = new(new() { Authority = "http://localhost" });

	private static IAuthenticationService AuthenticationService { get; } =
		new AuthenticationService(GnomeshadeClient, OidcClient);

	// Static member order is important due to initialization order
#pragma warning disable SA1202

	/// <summary>Gets an instance of <see cref="MainWindowViewModel"/> for use during design time.</summary>
	public static MainWindowViewModel MainWindowViewModel { get; } =
		new(GnomeshadeClient, AuthenticationService, DateTimeZoneProvider);

	/// <summary>Gets an instance of <see cref="AccountUpsertionViewModel"/> for use during design time.</summary>
	public static AccountUpsertionViewModel AccountUpsertionViewModel { get; } =
		AccountUpsertionViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="AccountViewModel"/> for use during design time.</summary>
	public static AccountViewModel AccountViewModel { get; } =
		AccountViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyViewModel"/> for use during design time.</summary>
	public static CounterpartyViewModel CounterpartyViewModel { get; } =
		CounterpartyViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyMergeViewModel"/> for use during design time.</summary>
	public static CounterpartyMergeViewModel CounterpartyMergeViewModel { get; } =
		CounterpartyMergeViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyUpdateViewModel"/> for use during design time.</summary>
	public static CounterpartyUpdateViewModel CounterpartyUpdateViewModel { get; } =
		CounterpartyUpdateViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="ImportViewModel"/> for use during design time.</summary>
	public static ImportViewModel ImportViewModel { get; } = new(GnomeshadeClient);

	/// <summary>Gets an instance of <see cref="LoginViewModel"/> for use during design time.</summary>
	public static LoginViewModel LoginViewModel { get; } = new(AuthenticationService);

	/// <summary>Gets an instance of <see cref="ProductCreationViewModel"/> for use during design time.</summary>
	public static ProductCreationViewModel ProductCreationViewModel { get; } =
		ProductCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="UnitCreationViewModel"/> for use during design time.</summary>
	public static UnitCreationViewModel UnitCreationViewModel { get; } =
		UnitCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TagCreationViewModel"/> for use during design time.</summary>
	public static TagCreationViewModel TagCreationViewModel { get; } =
		TagCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TagViewModel"/> for use during design time.</summary>
	public static TagViewModel TagViewModel { get; } =
		TagViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TransactionProperties"/> for use during design time.</summary>
	public static TransactionProperties TransactionProperties { get; } = new();

	/// <summary>Gets an instance of <see cref="ProductViewModel"/> for use during design time.</summary>
	public static ProductViewModel ProductViewModel { get; } =
		ProductViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="UnitViewModel"/> for use during design time.</summary>
	public static UnitViewModel UnitViewModel { get; } =
		UnitViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TransferUpsertionViewModel"/> for use during design time.</summary>
	public static TransferUpsertionViewModel TransferUpsertionViewModel { get; } =
		TransferUpsertionViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="TransferViewModel"/> for use during design time.</summary>
	public static TransferViewModel TransferViewModel { get; } =
		TransferViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="PurchaseUpsertionViewModel"/> for use during design time.</summary>
	public static PurchaseUpsertionViewModel PurchaseUpsertionViewModel { get; } =
		PurchaseUpsertionViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="PurchaseViewModel"/> for use during design time.</summary>
	public static PurchaseViewModel PurchaseViewModel { get; } =
		PurchaseViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="TransactionViewModel"/> for use during design time.</summary>
	public static TransactionViewModel TransactionViewModel { get; } =
		TransactionViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider).Result;

	/// <summary>Gets an instance of <see cref="TransactionFilter"/> for use during design time.</summary>
	public static TransactionFilter TransactionFilter { get; } =
		new() { FromDate = DateTimeOffset.Now, ToDate = DateTimeOffset.Now };

	/// <summary>Gets an instance of <see cref="TransactionUpsertionViewModel"/> for use during design time.</summary>
	public static TransactionUpsertionViewModel TransactionUpsertionViewModel { get; } =
		TransactionUpsertionViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="LinkUpsertionViewModel"/> for use during design time.</summary>
	public static LinkUpsertionViewModel LinkUpsertionViewModel { get; } =
		LinkUpsertionViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="LinkViewModel"/> for use during design time.</summary>
	public static LinkViewModel LinkViewModel { get; } =
		LinkViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;
}
