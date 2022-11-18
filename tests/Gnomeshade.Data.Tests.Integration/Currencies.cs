// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

using CsvHelper;
using CsvHelper.Configuration;

using JetBrains.Annotations;

namespace Gnomeshade.Data.Tests.Integration;

public sealed class Currencies
{
	[Test]
	public async Task GetCurrencyData()
	{
		await using var stream = Assembly
				.GetExecutingAssembly()
				.GetManifestResourceStream(typeof(Currencies), "currencies.csv") ??
			throw new MissingManifestResourceException();

		using var reader = new StreamReader(stream, Encoding.UTF8);
		using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			Delimiter = ";",
			ShouldSkipRecord = args => args.Row.CurrentIndex is not -1 &&
				(args.Row.GetField(3) is "" ||
				args.Row.GetField(5) is not ("" or null) ||
				args.Row.GetField(6) is not ("" or null)),
		});

		var data = csv
			.GetRecords<Row>()
			.Where(row => row.NumericCode is not (203 or 978 or 826 or 191 or 428 or 985 or 643 or 840 or 752 or 191))
			.Select(row =>
			{
				row.Name = row.Name?.Replace("'", "''");
				row.NormalizedName = row.NormalizedName?.Replace("'", "''");
				return row;
			})
			.Select(row =>
				$"('{row.Name}', '{row.NormalizedName}', {row.NumericCode}, '{row.AlphabeticCode}', {(int.TryParse(row.MinorUnit, out var minor) ? minor : 0)}, TRUE, FALSE, FALSE, NULL, NULL)");

		var values = string.Join($",{Environment.NewLine}", data);

		values.Should().NotBeNullOrWhiteSpace();
	}

	[UsedImplicitly(
		ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign,
		ImplicitUseTargetFlags.Members)]
	private sealed class Row
	{
		public string? Name { get; set; }

		public string? NormalizedName { get; set; }

		public string? AlphabeticCode { get; set; }

		public int NumericCode { get; set; }

		public string? MinorUnit { get; set; }
	}
}
