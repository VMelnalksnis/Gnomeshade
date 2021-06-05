using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Helpers
{
	public static class PasswordBoxHelper
	{
		public const string ParameterPropertyName = "Password";

		public static readonly DependencyProperty BoundPasswordProperty =
			DependencyProperty.RegisterAttached("BoundPassword",
				typeof(string),
				typeof(PasswordBoxHelper),
				new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

		public static string GetBoundPassword(DependencyObject dependencyObject)
		{
			if (dependencyObject is PasswordBox passwordBox)
			{
				// this funny little dance here ensures that we've hooked the
				// PasswordChanged event once, and only once.
				passwordBox.PasswordChanged -= PasswordChanged;
				passwordBox.PasswordChanged += PasswordChanged;
			}

			return (string)dependencyObject.GetValue(BoundPasswordProperty);
		}

		public static void SetBoundPassword(DependencyObject dependencyObject, string value)
		{
			if (string.Equals(value, GetBoundPassword(dependencyObject)))
			{
				return; // and this is how we prevent infinite recursion
			}

			dependencyObject.SetValue(BoundPasswordProperty, value);
		}

		private static void OnBoundPasswordChanged(
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs e)
		{
			if (dependencyObject is not PasswordBox box)
			{
				return;
			}

			box.Password = GetBoundPassword(dependencyObject);
		}

		private static void PasswordChanged(object sender, RoutedEventArgs e)
		{
			var password = (PasswordBox)sender;
			SetBoundPassword(password, password.Password);

			// set cursor past the last character in the password box
			var selectMethod = password.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);
			selectMethod.Invoke(password, new object[] { password.Password.Length, 0 });
		}
	}
}
