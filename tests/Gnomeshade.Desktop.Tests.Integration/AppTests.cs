// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Configuration;

using static Gnomeshade.Desktop.Tests.Integration.AvaloniaSetup;

namespace Gnomeshade.Desktop.Tests.Integration;

public sealed class AppTests
{
	[Test]
	public async Task InitialScreenShouldBeSettings()
	{
		var applicationLifetime = await GetApp();
		var window = applicationLifetime.MainWindow;

		await Invoke(() => window.IsVisible.Should().BeTrue());
		var viewModel = await Invoke(() => window.DataContext.Should().BeOfType<MainWindowViewModel>().Subject);
		viewModel.ActiveView.Should().BeOfType<ApplicationSettingsViewModel>();
	}
}
