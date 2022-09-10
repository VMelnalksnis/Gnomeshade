// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Importing;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Importing;

[TestOf(typeof(Iso20022Controller))]
public sealed class Iso20022ControllerTests : WebserverTests
{
	private const string _fileName = "BankToCustomerAccountReportV02.xml";

	private IGnomeshadeClient _client = null!;

	public Iso20022ControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[OneTimeSetUp]
	public async Task OneTimeSetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Import_ShouldReturnExpected()
	{
		await using var contentStream = Assembly
			.GetExecutingAssembly()
			.GetManifestResourceStream(typeof(Iso20022ControllerTests), _fileName)!;

		var reportResult = await _client.Import(contentStream, _fileName);

		using (new AssertionScope())
		{
			reportResult.Should().NotBeNull();
			reportResult.AccountReferences.Should().HaveCount(3);
			reportResult.TransferReferences.Should().HaveCount(2);
			reportResult.TransactionReferences.Should().HaveCount(2);
			reportResult.TransactionReferences.Select(reference => reference.Created).Should().AllBeEquivalentTo(true);
			reportResult.TransactionReferences.Select(reference => reference.Transaction.ImportedAt).Should().AllSatisfy(instant => instant.Should().NotBeNull());
		}

		contentStream.Seek(0, SeekOrigin.Begin);
		var secondReportResult = await _client.Import(contentStream, _fileName);

		secondReportResult.TransactionReferences.Select(reference => reference.Created).Should().AllBeEquivalentTo(false);
	}
}
