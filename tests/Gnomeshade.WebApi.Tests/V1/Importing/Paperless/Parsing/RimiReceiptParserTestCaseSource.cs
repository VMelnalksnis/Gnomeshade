// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Gnomeshade.WebApi.V1.Importing.Paperless.Parsing;

using Microsoft.Extensions.Logging.Abstractions;

using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.Tests.V1.Importing.Paperless.Parsing;

[SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Contains test data")]
public sealed class RimiReceiptParserTestCaseSource : IEnumerable<TestCaseData>
{
	public IEnumerator<TestCaseData> GetEnumerator()
	{
		var parser = new RimiReceiptParser(NullLogger<RimiReceiptParser>.Instance);

		var standardProductPart = @"Tualetes papire Zewa Delicate
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

1 gab X 2,99 EUR 2,99 8".ReplaceLineEndings("\n");

		var expectedProducts = new List<string>
		{
			@"Tualetes papire Zewa Delicate
Care, gab
1 gab X 4,99 EUR 4,99 8
Atl -2,00 Gala cena 2,99",

			@"Tostermaize franēu
Brioche 450g
1 gab x 2,55 EUR 2,55 8",

			@"Sviests Exporta 82,5% 200g
1 gab X 3,09 EUR 3,09 A
Atl -0,50 Gala cena 2,59",

			@"Sviests Smltene 82% 200g
1 gab X 2,99 EUR 2,99 8",
		};

		yield return new TestCaseData(
				parser,
				new Document
				{
					Content = $@"RimilY

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



{standardProductPart}





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
				},
				expectedProducts)
			.SetName("Multiple newline groups before products");

		yield return new TestCaseData(
				parser,
				new Document
				{
					Content = $@"RimiV

Katra diena arvien tabaka

SIA RIME LATVIA
Jur. adrese “Riga, A. Deglava iela 161
Rimi Super Agenskalns (Riga)

Kase Nr. 31

PVN _maksataja numurs. LVv40001234567
Sasijas nymure: SP-LVO0123
= Ceks —
= Elektromiska izdruka
KLIENTS. YXOCCCOOOORIOKE S16





{standardProductPart}



ATLALDES
Citas akcijas






Tavs 1etaupijuns

Maksajumu karte
Apnakaa
BEZKONTAKTA KARTE
VISA BEZKONTAKTA

BANKAS KVITS NR 123456
TERMINALA ID 12345678
TIRGOPAJA ID 1234567
LALKS 2020-01-13 12 59 58".ReplaceLineEndings("\n"),
				},
				expectedProducts)
			.SetName("Single newline group before products");

		yield return new TestCaseData(
				parser,
				new Document
				{
					Content = $@"RimivV
Katra diena arvien labaka

SIA RIME LATVIA
Jur. adrese: Riga, A. Deglava iela 161
Rimi Super Agenskalns (Riga)

Kase Nr. 35

PVN makeataja nunurs: LVv40001234567
Sasijas nymurs: SP-LVO0123
Ceks —
Elektroniska izdruka
OOOCCCOOOOGIOKES 16







{standardProductPart}
ATLALDES

Tamantota Mans Rimi nauda -1,31
Gites akes jas 27138
Tavs 1etaupijuns 8,59



Makeajumu karte
Apnaksa
BEZKONTAKTA KARTE
VISA BEZKONTAKTA

BBANKAS KVITS NR 123456
TERMINALA ID 12345678
TIRGOPAJA ID 1234567
LALKS 2020-01-13 12 59 58".ReplaceLineEndings("\n"),
				},
				expectedProducts)
			.SetName("Discounts immediately after products");
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
