// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Input.Platform;

using Gnomeshade.Avalonia.Core.Commands;

namespace Gnomeshade.Avalonia.Core.Help;

/// <summary>Information about the application and the system.</summary>
public sealed class AboutViewModel : ViewModelBase
{
	private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

	/// <summary>Initializes a new instance of the <see cref="AboutViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	public AboutViewModel(IActivityService activityService)
		: base(activityService)
	{
		Version = _assembly.GetName().Version?.ToString() ?? throw new NullReferenceException();
		DotnetVersion = RuntimeInformation.FrameworkDescription;
		OsVersion = RuntimeInformation.OSDescription;

		Cores = $"{Environment.ProcessorCount} ({RuntimeInformation.ProcessArchitecture})";

		var memoryInfo = GC.GetGCMemoryInfo();
		MemoryMegabytes = memoryInfo.TotalAvailableMemoryBytes / 1024 / 1024;

		GarbageCollector = string.Join(
			"; ",
			GCSettings.IsServerGC ? "Server" : "Workstation",
			memoryInfo.Concurrent ? "Concurrent" : "Non-Concurrent");

		Copy = activityService.Create<IClipboard?>(GetFullText, "Copying application information");
	}

	/// <summary>Gets the version of the application.</summary>
	public string Version { get; }

	/// <summary>Gets the version on the .NET runtime.</summary>
	public string DotnetVersion { get; }

	/// <summary>Gets the version of the operating system.</summary>
	public string OsVersion { get; }

	/// <summary>Gets the count and architecture of CPU cores.</summary>
	public string Cores { get; }

	/// <summary>Gets the total available memory in megabytes.</summary>
	public long MemoryMegabytes { get; }

	/// <summary>Gets the garbage collector type.</summary>
	public string GarbageCollector { get; }

	/// <summary>Gets a command for setting the clipboard to application information.</summary>
	public CommandBase Copy { get; }

	private async Task GetFullText(IClipboard? clipboard)
	{
		ArgumentNullException.ThrowIfNull(clipboard);

		var memoryInfo = GC.GetGCMemoryInfo();

		var builder = new StringBuilder();

		builder.AppendLine($"Gnomeshade {Version}");
		builder.AppendLine(OsVersion);
		builder.AppendLine(DotnetVersion);
		builder.AppendLine(
			$"GC: {(GCSettings.IsServerGC ? "Server" : "Workstation")} ({(memoryInfo is { Concurrent: true } ? "Concurrent" : "Non-Concurrent")})");
		builder.AppendLine($"GC latency: {GCSettings.LatencyMode}");
		builder.AppendLine($"GC LOH compaction: {GCSettings.LargeObjectHeapCompactionMode}");
		builder.AppendLine($"Memory: {MemoryMegabytes}M");
		builder.AppendLine($"CPU: {Environment.ProcessorCount} cores ({RuntimeInformation.ProcessArchitecture})");

		var text = builder.ToString();
		await clipboard.SetTextAsync(text);
	}
}
