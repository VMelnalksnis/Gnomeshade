using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class Currency
	{
		public int Id { get; set; }

		public short NumericCode { get; set; }

		public string AlphabeticCode { get; set; }

		public short MinorUnit { get; set; }

		public string Name { get; set; }

		public bool Official { get; set; }

		public bool Crypto { get; set; }

		public bool Historical { get; set; }

		public DateTimeOffset? From { get; set; }

		public DateTimeOffset? Until { get; set; }
	}
}
