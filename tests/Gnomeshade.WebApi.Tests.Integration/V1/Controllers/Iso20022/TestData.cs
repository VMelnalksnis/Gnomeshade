// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers.Iso20022;

internal static class TestData
{
	private static readonly Type _namespaceType = typeof(TestData);
	private static readonly Assembly _assembly = _namespaceType.Assembly;

	internal static TestCase Sample => new(
		GetResourceStream("BankToCustomerAccountReportV02.xml"),
		"LV12TEST000000000001");

	internal static TestCase Account1 => new(
		GetResourceStream("Account1.xml"),
		"LV22TEST0012345678901");

	internal static TestCase Account2 => new(
		GetResourceStream("Account2.xml"),
		"LV22TEST0012345678903");

	private static Stream GetResourceStream(string name)
	{
		var stream = _assembly.GetManifestResourceStream(_namespaceType, name);
		if (stream is not null)
		{
			return stream;
		}

		var message = $"Could not find resource {name} in namespace {_namespaceType.Namespace}";
		throw new MissingManifestResourceException(message);
	}

	internal sealed record TestCase(Stream Stream, string AccountIban);
}
