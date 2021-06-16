// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Text;

using Bogus.DataSets;

namespace Tracking.Finance.Interfaces.WebApi.Tests.Integration
{
	public static class BogusExtensions
	{
		public static string Password(this Internet internet, int minLength, int maxLength)
		{
			var randomizer = internet.Random;
			var stringBuilder = new StringBuilder(maxLength)
				.Append(randomizer.Char('a', 'z'))
				.Append(randomizer.Char('A', 'Z'))
				.Append(randomizer.Char('0', '9'))
				.Append(randomizer.Char('!', '/'))
				.Append(randomizer.String2(minLength - 4))
				.Append(randomizer.String2(randomizer.Number(0, maxLength - minLength)));

			var shuffledCharacters = randomizer.Shuffle(stringBuilder.ToString()).ToArray();
			return new(shuffledCharacters);
		}
	}
}
