// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Globalization;
using System.Windows.Data;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Helpers
{
	/// <summary>
	/// An implementation of <see cref="Binding"/> which uses <see cref="CultureInfo.CurrentCulture"/>
	/// instead of a value specified at design time.
	/// </summary>
	///
	/// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.data.binding.converterculture?view=net-5.0#remarks"/>
	/// <see href="https://stackoverflow.com/a/5937477"/>
	public sealed class CultureAwareBinding : Binding
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CultureAwareBinding"/> class.
		/// </summary>
		public CultureAwareBinding()
		{
			ConverterCulture = CultureInfo.CurrentCulture;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CultureAwareBinding"/> class with an initial path.
		/// </summary>
		/// <param name="path">The initial <see cref="Binding.Path"/> for the binding.</param>
		public CultureAwareBinding(string path)
			: base(path)
		{
			ConverterCulture = CultureInfo.CurrentCulture;
		}
	}
}
