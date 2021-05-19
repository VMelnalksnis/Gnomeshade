namespace Tracking.Finance.Web.Models.Units
{
	public class UnitIndexModel
	{
		public UnitIndexModel(int id, string name, short exponent, decimal mantissa)
		{
			Id = id;
			Name = name;
			Exponent = exponent;
			Mantissa = mantissa;
		}

		public int Id { get; }

		public string Name { get; }

		public short Exponent { get; }

		public decimal Mantissa { get; }
	}
}
