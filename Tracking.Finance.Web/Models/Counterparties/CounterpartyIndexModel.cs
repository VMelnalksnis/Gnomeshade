namespace Tracking.Finance.Web.Models.Counterparties
{
	public class CounterpartyIndexModel
	{
		public CounterpartyIndexModel(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get; }

		public string Name { get; }
	}
}
