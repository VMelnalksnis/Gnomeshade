// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Importing
{
	public class Iso20022ControllerTests
	{
		private IGnomeshadeClient _client = null!;
		private FileInfo _inputFile = null!;

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			_client = await WebserverSetup.CreateAuthorizedClientAsync();

			var filePath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"V1_0/Importing/",
				"BankToCustomerAccountReportV02.xml");

			_inputFile = new(filePath);
		}

		[Test]
		public async Task Import_ShouldReturnExpected()
		{
			await using var contentStream = _inputFile.OpenRead();
			var reportResult = await _client.Import(contentStream, _inputFile.Name);

			reportResult.Should().NotBeNull();

			reportResult.AccountReferences.Should().HaveCount(3);
			reportResult.ProductReferences.Should().HaveCount(2);
			reportResult.TransactionReferences.Should().HaveCount(2);
		}
	}
}
