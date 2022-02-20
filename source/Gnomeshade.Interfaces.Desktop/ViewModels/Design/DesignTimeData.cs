// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Reflection;

using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;

using Gnomeshade.Interfaces.Desktop.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Design;

/// <summary>
/// Data needed only during design time.
/// </summary>
public static class DesignTimeData
{
	private static DesignTimeGnomeshadeClient GnomeshadeClient { get; } = new();

	private static DesignTimeOAuth2Client OAuth2Client { get; } = new();

	private static IAuthenticationService AuthenticationService { get; } =
		new AuthenticationService(GnomeshadeClient, OAuth2Client);

	/// <summary>
	/// Gets an instance of <see cref="MainWindowViewModel"/> for use during design time.
	/// </summary>
	public static MainWindowViewModel MainWindowViewModel { get; } = new(GnomeshadeClient, AuthenticationService);

	/// <summary>
	/// Gets an instance of <see cref="AccountCreationViewModel"/> for use during design time.
	/// </summary>
	public static AccountCreationViewModel AccountCreationViewModel { get; } =
		AccountCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>
	/// Gets an instance of <see cref="AccountDetailViewModel"/> for use during design time.
	/// </summary>
	public static AccountDetailViewModel AccountDetailViewModel { get; } =
		AccountDetailViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>
	/// Gets an instance of <see cref="AccountViewModel"/> for use during design time.
	/// </summary>
	public static AccountViewModel AccountViewModel { get; } =
		AccountViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>
	/// Gets an instance of <see cref="ImportViewModel"/> for use during design time.
	/// </summary>
	public static ImportViewModel ImportViewModel { get; } = new(GnomeshadeClient);

	/// <summary>
	/// Gets an instance of <see cref="LoginViewModel"/> for use during design time.
	/// </summary>
	public static LoginViewModel LoginViewModel { get; } = new(AuthenticationService);

	/// <summary>
	/// Gets an instance of <see cref="ProductCreationViewModel"/> for use during design time.
	/// </summary>
	public static ProductCreationViewModel ProductCreationViewModel { get; } =
		ProductCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>
	/// Gets an instance of <see cref="TransactionCreationViewModel"/> for use during design time.
	/// </summary>
	public static TransactionCreationViewModel TransactionCreationViewModel { get; } = new(GnomeshadeClient);

	/// <summary>
	/// Gets an instance of <see cref="TransactionItemCreationViewModel"/> for use during design time.
	/// </summary>
	public static TransactionItemCreationViewModel TransactionItemCreationViewModel { get; } =
		TransactionItemCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	internal static TransactionItemSplitViewModel TransactionItemSplitViewModel { get; } =
		TransactionItemSplitViewModel.CreateAsync(
			GnomeshadeClient,
			new()
			{
				SourceAccount = "Credit",
				SourceCurrency = "EUR",
				SourceAmount = 123.45m,
				TargetAccount = "Amazon",
				TargetCurrency = "EUR",
				TargetAmount = 123.45m,
				Product = "Bread",
				Amount = 0.5m,
			},
			Guid.Empty).Result;

	/// <summary>
	/// Gets an instance of <see cref="TransactionDetailViewModel"/> for use during design time.
	/// </summary>
	public static TransactionDetailViewModel TransactionDetailViewModel { get; } =
		TransactionDetailViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>
	/// Gets an instance of <see cref="TransactionViewModel"/> for use during design time.
	/// </summary>
	public static TransactionViewModel TransactionViewModel { get; } =
		TransactionViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>
	/// Gets an instance of <see cref="UnitCreationViewModel"/> for use during design time.
	/// </summary>
	public static UnitCreationViewModel UnitCreationViewModel { get; } =
		UnitCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>
	/// Forces the loading of assemblies that are needed during design time, but are not automatically included.
	/// </summary>
	/// <remarks>
	/// Assemblies that are not directly referenced are not loaded at design time, but are at compile time.
	/// </remarks>
	public static void ForceAssembliesToLoad()
	{
		typeof(Product).GetTypeInfo();
		typeof(Interaction).GetTypeInfo();
		typeof(EventTriggerBehavior).GetTypeInfo();
	}
}
