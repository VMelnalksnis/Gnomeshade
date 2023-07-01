// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core.Help;

/// <summary>Information about a dependency.</summary>
public sealed class PackageInfo : PropertyChangedBase
{
	/// <summary>Gets or sets the unique package identifier.</summary>
	public string PackageId { get; set; } = null!;

	/// <summary>Gets or sets the license URL or SPDX identifier.</summary>
	public string License { get; set; } = null!;

	/// <summary>Gets or sets the URL to the package project.</summary>
	public Uri? PackageProjectUrl { get; set; }

	/// <summary>Gets or sets the version of the package.</summary>
	public string PackageVersion { get; set; } = null!;

	/// <summary>Gets or sets the author of the package.</summary>
	public string? Author { get; set; }

	internal static async Task<List<PackageInfo>> ReadAsync()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var resource = assembly.GetManifestResourceStream(typeof(PackageInfo), "licenses.json");
		if (resource is null)
		{
			throw new MissingManifestResourceException("Could not find licenses.json");
		}

		var reader = new StreamReader(resource);
		var text = await reader.ReadToEndAsync();
		var context = new PackageContext(new(JsonSerializerDefaults.Web));
		return JsonSerializer
			.Deserialize(text, context.ListPackageInfo)!
			.GroupBy(info => info.PackageId)
			.Select(grouping => grouping.OrderByDescending(x => x.PackageVersion).First())
			.ToList();
	}
}
