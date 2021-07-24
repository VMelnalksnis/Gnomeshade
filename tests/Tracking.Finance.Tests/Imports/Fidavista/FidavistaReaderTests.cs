// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Imports.Fidavista;

namespace Tracking.Finance.Tests.Imports.Fidavista
{
	public class FidavistaReaderTests
	{
		[Test]
		public void ReadEmptyFile()
		{
			var content = TestFiles.Statementv2;
			using var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(content);
			writer.Flush();
			stream.Position = 0;

			var document = new FidavistaReader().Read(stream);

			document.Header.Should().BeNull();
			document.Statements.Should().BeNull();
		}
	}
}
