// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Design
{
	/// <summary>
	/// Data needed only during design time.
	/// </summary>
	public static class DesignTimeData
	{
		/// <summary>
		/// Gets a Gnomeshade API client for use during design time.
		/// </summary>
		public static DesignTimeGnomeshadeClient GnomeshadeClient { get; } = new();

		/// <summary>
		/// Gets an instance of <see cref="AccountCreationViewModel"/> for use during design time.
		/// </summary>
		public static AccountCreationViewModel AccountCreationViewModel { get; } =
			AccountCreationViewModel.CreateAsync(GnomeshadeClient).Result;

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
		public static LoginViewModel LoginViewModel { get; } = new(GnomeshadeClient);

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
	}
}
