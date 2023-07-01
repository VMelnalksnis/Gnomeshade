// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core.Help;

/// <inheritdoc cref="IComparer{T}" />
internal sealed class PackageInfoComparer : IComparer<PackageInfo?>, IComparer
{
	/// <inheritdoc />
	public int Compare(object? x, object? y) => Compare(x as PackageInfo, y as PackageInfo);

	/// <inheritdoc />
	public int Compare(PackageInfo? x, PackageInfo? y)
	{
		if (ReferenceEquals(x, y))
		{
			return 0;
		}

		if (ReferenceEquals(null, y))
		{
			return 1;
		}

		if (ReferenceEquals(null, x))
		{
			return -1;
		}

		var projectComparison = Uri.Compare(
			x.PackageProjectUrl,
			y.PackageProjectUrl,
			UriComponents.AbsoluteUri,
			UriFormat.Unescaped,
			StringComparison.OrdinalIgnoreCase);

		if (projectComparison is not 0)
		{
			return projectComparison;
		}

		var packageIdComparison = string.Compare(x.PackageId, y.PackageId, StringComparison.OrdinalIgnoreCase);
		if (packageIdComparison is not 0)
		{
			return packageIdComparison;
		}

		var licenseComparison = string.Compare(x.License, y.License, StringComparison.OrdinalIgnoreCase);
		if (licenseComparison is not 0)
		{
			return licenseComparison;
		}

		var packageVersionComparison =
			string.Compare(x.PackageVersion, y.PackageVersion, StringComparison.OrdinalIgnoreCase);
		if (packageVersionComparison is not 0)
		{
			return packageVersionComparison;
		}

		return string.Compare(x.Author, y.Author, StringComparison.OrdinalIgnoreCase);
	}
}
