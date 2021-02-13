namespace Tracking.Finance.Web.Data.Models
{
	public class UnitClosure
	{
		public int Id { get; set; }

		public int ParentUnitId { get; set; }

		public int ChildUnitId { get; set; }

		public byte Depth { get; set; }

		public Unit ParentUnit { get; set; }

		public Unit ChildUnit { get; set; }
	}
}
