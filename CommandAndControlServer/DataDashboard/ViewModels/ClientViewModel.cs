namespace DataDashboard.ViewModels
{
	public class ClientViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string LastIP { get; set; }
		public DateTime LastConnectionTime { get; set; }
		public string CpuId { get; set; }
		public string MAC {  get; set; }
		public string RAMCapacity { get; set; }
		public string OS { get; set; }
	}
}
