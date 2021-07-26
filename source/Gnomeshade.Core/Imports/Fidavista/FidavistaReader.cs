// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using System.Xml.Serialization;

namespace Gnomeshade.Core.Imports.Fidavista
{
	public sealed class FidavistaReader
	{
		private readonly XmlSerializer _xmlSerializer = new(typeof(FidavistaDocument));

		public FidavistaDocument Read(Stream stream)
		{
			return (FidavistaDocument)_xmlSerializer.Deserialize(stream)!;
		}
	}
}
