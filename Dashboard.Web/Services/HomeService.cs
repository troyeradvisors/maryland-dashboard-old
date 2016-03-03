using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using Dashboard.Web;

namespace Dashboard.Web.Services
{



    // Implements application logic using the DashboardEntities context.
    // TODO: Add your application logic to these methods or in additional methods.
    // TODO: Wire up authentication (Windows/ASP.NET Forms) and uncomment the following to disable anonymous access
    // Also consider adding roles to restrict access as appropriate.
    // [RequiresAuthentication]
    [EnableClientAccess()]
    public class HomeService : LinqToEntitiesDomainService<DashboardEntities>
    {
        public IEnumerable<int> GetFiscalYears() { return GetDescriptions().Select(e => e.FiscalYear).Distinct().OrderByDescending(e => e); }


        public IQueryable<HomeSummary> GetHomeSummaries() 
        {
            return from home in GetHomes()
                   join description in GetDescriptions() on home.PIN equals description.PIN
                   orderby home.Name
                   select new HomeSummary() { Name = home.Name, PIN = home.PIN, FiscalYear = description.FiscalYear, CountyCode = home.CountyCode };
        }

        public IQueryable<HomeSummary> GetHomeSummariesByYear(int year)
        {
            return GetHomeSummaries().Where(e=>e.FiscalYear == year);
        }

		public IQueryable<FinanceSummary> GetFinanceSummary()
		{
			return
				from finance in GetFinances()
				join home in GetHomes() on finance.PIN equals home.PIN
				orderby home.Name
				select new FinanceSummary()
				       	{
				       		PIN = home.PIN,
				       		FiscalYear = finance.FiscalYear,
				       		Name = home.Name,
				       		Street = home.Street,
				       		State = home.State,
				       		City = home.City,
				       		Beds =  finance.CertifiedBedCount,
				       		Occupancy = (double) finance.NursingOccupancyPercent,
				       		Expense = (double) finance.TotalExpense,
				       		Revenue = (double) finance.TotalRevenue,
				       		Income = (double) finance.NetIncome,
							ProfitMargin = (double) finance.NetIncomePerRevenuePercent,
							MedicareRevenuePerDay = (double) finance.MedicareRevenuePerDay
				       	};
		}

        public IQueryable<Home> GetHomes() { return ObjectContext.Homes.Where(e=>ObjectContext.Descriptions.Any(f =>e.PIN==f.PIN)); }
        public IQueryable<Home> GetHomesByPin(int pin) { return GetHomes().Where(e=>e.PIN==pin); }
        public IQueryable<Home> GetHomesByPins(string pins)
        {
            List<int> p = pins.Split(' ').Select(e => int.Parse(e)).ToList();
            return GetHomes().Where(x => p.Contains(x.PIN));
        }

        public IQueryable<CountyAverage> GetCountyAverages() { return ObjectContext.CountyAverages; }
        public IQueryable<Description> GetDescriptions() { return this.ObjectContext.Descriptions; }
        public IQueryable<StateAverage> GetStateAverages() { return ObjectContext.StateAverages; }
        public IQueryable<Summary> GetFinances() { return ObjectContext.Summaries; }
        public IQueryable<Summary> GetFinancesByPins(string pins, int fiscalYear)
        {
            List<int> p = pins.Split(' ').Select(e=>int.Parse(e)).ToList();
            return ObjectContext.Summaries.Where(x => x.FiscalYear == fiscalYear && p.Contains(x.PIN));
        }


    }
}


