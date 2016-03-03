using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Dashboard.Web
{
	public partial class FinanceSummary
	{
		public Home Home;
		public Summary Finance;
		public string Address { get { return this.City + ", " + this.State;  } }
		public FinanceSummary(Home home, Summary finance)
		{
			Home = home;
			Finance = finance;
			this.PIN = home.PIN;
			this.Name = home.Name;
			this.FiscalYear = finance.FiscalYear;
			this.Street = home.Street;
			this.City = home.City;
			this.State = home.State;
			Beds = finance.CertifiedBedCount;
			Occupancy = Convert.ToDouble(finance.NursingOccupancyPercent);
			Expense = Convert.ToDouble(finance.TotalExpense);
			Revenue = Convert.ToDouble(finance.TotalRevenue);
			Income = Convert.ToDouble(finance.NetIncome);
		}
	}
}
