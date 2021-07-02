// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Reactive;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI;

using Tracking.Finance.Interfaces.WebApi.Client;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class MainWindowViewModel : ViewModelBase
	{
		private ViewModelBase _activeView;

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
		/// </summary>
		public MainWindowViewModel()
		{
			_activeView = new LoginViewModel(this, new FinanceClient());
			ExitCommand = ReactiveCommand.Create(Exit);
		}

		/// <summary>
		/// Gets or sets the currently active view.
		/// </summary>
		public ViewModelBase ActiveView
		{
			get => _activeView;
			set => this.RaiseAndSetIfChanged(ref _activeView, value, nameof(ActiveView));
		}

		/// <summary>
		/// Gets the <see cref="ReactiveCommand{TParam,TResult}"/> for exiting the application.
		/// </summary>
		public ReactiveCommand<Unit, Unit> ExitCommand { get; }

		private static void Exit()
		{
			var desktopLifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
			desktopLifetime.Shutdown();
		}
	}
}
