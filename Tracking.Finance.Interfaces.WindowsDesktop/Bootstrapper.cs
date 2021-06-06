using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;
using Tracking.Finance.Interfaces.WindowsDesktop.Input;
using Tracking.Finance.Interfaces.WindowsDesktop.Models;
using Tracking.Finance.Interfaces.WindowsDesktop.ViewModels;

namespace Tracking.Finance.Interfaces.WindowsDesktop
{
	/// <summary>
	/// Caliburn Micro configuration.
	/// </summary>
	public sealed class Bootstrapper : BootstrapperBase
	{
		private readonly SimpleContainer _container = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="Bootstrapper"/> class.
		/// </summary>
		public Bootstrapper()
		{
			Initialize();

			_ = ConventionManager
				.AddElementConvention<PasswordBox>(
					PasswordBoxHelper.BoundPasswordProperty,
					PasswordBoxHelper.ParameterPropertyName,
					nameof(PasswordBox.PasswordChanged));
		}

		/// <inheritdoc/>
		protected sealed override void Configure()
		{
			_ = _container
				.Instance(_container)
				.Singleton<IWindowManager, WindowManager>()
				.Singleton<IEventAggregator, EventAggregator>()
				.Singleton<IFinanceClient, FinanceClient>()
				.Singleton<LoggedInUserModel>();

			foreach (var viewModelType in IViewModel.GetViewModels<Bootstrapper>())
			{
				_container.RegisterPerRequest(viewModelType, viewModelType.ToString(), viewModelType);
			}

			var defaultCreateTrigger = Parser.CreateTrigger;
			Parser.CreateTrigger = (target, triggerText) =>
			{
				if (triggerText is null)
				{
					return defaultCreateTrigger(target, null);
				}

				var triggerDetail = triggerText.Replace("[", string.Empty).Replace("]", string.Empty);
				var details = triggerDetail.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
				var actionType = details[0];
				var action = details[1];
				var converter = new MultiKeyGestureConverter();

				return actionType switch
				{
					"Key" => new KeyTrigger { Key = (Key)Enum.Parse(typeof(Key), action, true) },
					"Gesture" => new KeyTrigger
					{
						Modifiers = ((MultiKeyGesture)converter.ConvertFrom(action)).KeySequences.First().Modifiers,
						Key = ((MultiKeyGesture)converter.ConvertFrom(action)).KeySequences.First().Keys.First(),
					},
					_ => defaultCreateTrigger(target, triggerText),
				};
			};
		}

		/// <inheritdoc/>
		protected sealed override void OnStartup(object sender, StartupEventArgs e) => DisplayRootViewFor<ShellViewModel>();

		/// <inheritdoc/>
		protected sealed override object GetInstance(Type service, string key) => _container.GetInstance(service, key);

		/// <inheritdoc/>
		protected sealed override IEnumerable<object> GetAllInstances(Type service) => _container.GetAllInstances(service);

		/// <inheritdoc/>
		protected sealed override void BuildUp(object instance) => _container.BuildUp(instance);
	}
}
