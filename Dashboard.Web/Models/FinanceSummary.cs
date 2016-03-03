namespace Dashboard.Web
{
	using System.Runtime.Serialization;
	using System.ServiceModel.DomainServices.Server.ApplicationServices;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// Class containing information about the authenticated user.
	/// </summary>
	public partial class FinanceSummary
	{
		[Key]
		[Editable(false)]
		public int PIN { get; set; }
		[Key]
		[Editable(false)]
		public int FiscalYear { get; set; }

		public string Name { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string State { get; set; }
		public double Occupancy { get; set; }
		public int Beds { get;set; }
		public double Revenue { get;set; }
		public double Expense { get; set; }
		public double Income { get; set; }
		public double ProfitMargin { get; set; }
		public double MedicareRevenuePerDay { get; set; }
	}

}