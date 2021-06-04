using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;
using Tracking.Finance.Interfaces.WindowsDesktop.ViewModels;

namespace Tracking.Finance.Interfaces.WindowsDesktop
{
	public sealed class Bootstrapper : BootstrapperBase
	{
		private SimpleContainer _container = new SimpleContainer();

		public Bootstrapper()
		{
			Initialize();

			ConventionManager.AddElementConvention<PasswordBox>(
			PasswordBoxHelper.BoundPasswordProperty,
			"Password",
			"PasswordChanged");
		}

		protected sealed override void Configure()
		{
			_container.Instance(_container);

			_container
				.Singleton<IWindowManager, WindowManager>()
				.Singleton<IEventAggregator, EventAggregator>();

			var assemblyTypes = GetType().Assembly.GetTypes();
			var viewModelTypes = assemblyTypes.Where(type => type.IsClass && type.Name.EndsWith("ViewModel"));
			foreach (var viewModelType in viewModelTypes)
			{
				_container.RegisterPerRequest(viewModelType, viewModelType.ToString(), viewModelType);
			}
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
