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

using IdentityModel.OidcClient;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <summary>Data needed only during design time.</summary>
public static class DesignTimeData
{
	private static DesignTimeGnomeshadeClient GnomeshadeClient { get; } = new();

	private static OidcClient OidcClient { get; } = new(new() { Authority = "http://localhost" });

	private static IAuthenticationService AuthenticationService { get; } =
		new AuthenticationService(GnomeshadeClient, OidcClient);

	// Static member order is important due to initialization order
#pragma warning disable SA1202

	/// <summary>Gets an instance of <see cref="MainWindowViewModel"/> for use during design time.</summary>
	public static MainWindowViewModel MainWindowViewModel { get; } = new(GnomeshadeClient, AuthenticationService);

	/// <summary>Gets an instance of <see cref="AccountCreationViewModel"/> for use during design time.</summary>
	public static AccountCreationViewModel AccountCreationViewModel { get; } =
		AccountCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="AccountDetailViewModel"/> for use during design time.</summary>
	public static AccountDetailViewModel AccountDetailViewModel { get; } =
		AccountDetailViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

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

	/// <summary>Gets an instance of <see cref="TransactionCreationViewModel"/> for use during design time.</summary>
	public static TransactionCreationViewModel TransactionCreationViewModel { get; } = new(GnomeshadeClient);

	/// <summary>Gets an instance of <see cref="TransactionItemCreationViewModel"/> for use during design time.</summary>
	public static TransactionItemCreationViewModel TransactionItemCreationViewModel { get; } =
		TransactionItemCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TransactionItemSplitViewModel"/> for use during design time.</summary>
	public static TransactionItemSplitViewModel TransactionItemSplitViewModel { get; } =
		TransactionItemSplitViewModel.CreateAsync(
			GnomeshadeClient,
			new(
				new()
				{
					SourceAmount = 123.45m,
					TargetAmount = 123.45m,
					Product = new() { Name = "Bread" },
					Amount = 0.5m,
				},
				new() { Name = "Credit" },
				null,
				new() { AlphabeticCode = "EUR" },
				new() { Name = "Amazon DE 123" },
				new() { Name = "Amazon" },
				new() { AlphabeticCode = "EUR" },
				new() { "Food" }),
			Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="TransactionDetailViewModel"/> for use during design time.</summary>
	public static TransactionDetailViewModel TransactionDetailViewModel { get; } =
		TransactionDetailViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="TransactionViewModel"/> for use during design time.</summary>
	public static TransactionViewModel TransactionViewModel { get; } =
		TransactionViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="UnitCreationViewModel"/> for use during design time.</summary>
	public static UnitCreationViewModel UnitCreationViewModel { get; } =
		UnitCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TagCreationViewModel"/> for use during design time.</summary>
	public static TagCreationViewModel TagCreationViewModel { get; } =
		TagCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TagViewModel"/> for use during design time.</summary>
	public static TagViewModel TagViewModel { get; } =
		TagViewModel.CreateAsync(GnomeshadeClient).Result;
}
