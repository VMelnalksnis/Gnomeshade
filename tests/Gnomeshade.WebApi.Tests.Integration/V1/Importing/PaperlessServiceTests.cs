// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Importing.Paperless;

using Microsoft.Extensions.DependencyInjection;

using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Importing;

public sealed class PaperlessServiceTests : WebserverTests
{
	public PaperlessServiceTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task Importing_ShouldRestoreDeletedProducts()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();
		var transaction = await client.CreateTransactionAsync();
		var document = new Document
		{
			Content = @"RimilY

Katra diena arvien labaka

SIA RIME LATVIA
Jur adrese Riga, A Deglava iela 161
Rimi Super Agenskalns (Riga)

Kase Nr 33,

PVN makeataja numurs LVv40001234567
Sasijas mymurs SP-LVO0123
Ceks —
Elektroniska izdruka
GOCHNRCOCONGNONE S16





KLIENTS



Tualetes papire Zewa Delicate
Care, gab

1 gab X 4,99 EUR 4,99 8
Atl -2,00 Gala cena 2,99
Tostermaize franéu

Brioche 450g

1 gab x 2,55 EUR 2,55 8

Sviests Exporta 82,5% 200g

1 gab X 3,09 EUR 3,09 A
Atl -0,50 Gala cena 2,59

Sviests Smltene 82% 200g

1 gab X 2,99 EUR 2,99 8





ATLALDES

Tualetes papirs Zewa Delicate
Care, 8gab -2,00
Izmantota Mans Rim nauda -0,81
Citas akerjas =1) 66



Tavs 1etaupijuns 6,21



Makeajanu karte
Apnakaa

BEZKONTAKTA KARTE
VISA BEZKONTAKTA

BANKAS KVITS NR 123456
TERMINALA ID 12345678
TIRGOPAJA ID 1234567
LALKS 2020-01-13 12 59 58".ReplaceLineEndings("\n"),
		};

		using var scope = Fixture.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IPaperlessService>();

		var purchases = await client.GetPurchasesAsync(transaction.Id);
		purchases.Should().BeEmpty();

		await service.AddPurchasesToTransaction(transaction.OwnerId, transaction.Id, document);
		purchases = await client.GetPurchasesAsync(transaction.Id);
		purchases.Should().HaveCount(4);

		await client.DeletePurchaseAsync(purchases.First().Id);

		purchases = await client.GetPurchasesAsync(transaction.Id);
		purchases.Should().HaveCount(3);

		await service.AddPurchasesToTransaction(transaction.OwnerId, transaction.Id, document);
		purchases = await client.GetPurchasesAsync(transaction.Id);
		purchases.Should().HaveCount(4);
	}
}
