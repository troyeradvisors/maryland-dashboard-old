using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using Dashboard.Web;
using Dashboard.Web.Services;
using System.ComponentModel;

namespace Dashboard
{
	public class Data : INotifyPropertyChanged
	{
		public static HomeContext HomeContext { get; set; }
		public static HubContext HubContext = new HubContext();



        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

		public static EntitySet<HomeSummary> Summaries
		{
			get { return HomeContext.HomeSummaries; }
		}
        public static IEnumerable<HomeSummary> SummariesThisYear { get { return Summaries.Where(e => e.FiscalYear == CurrentFiscalYear); } }

		public static Client CurrentClient;
		public static void LoadCurrentClient(EventHandler action)
		{
			LoadOperation<Client> lo = HubContext.Load(HubContext.GetCurrentClientQuery());
			lo.Completed += (x, y) =>
				{
					CurrentClient = lo.Entities.FirstOrDefault() as Client;
					action(null, null);
				};
		}

        public static List<int> FiscalYears = new List<int>();
        public static void LoadFiscalYears(EventHandler action)
        {
            InvokeOperation<IEnumerable<int>> invoke = HomeContext.GetFiscalYears();
            invoke.Completed += (x, y) =>
                {
                    FiscalYears = invoke.Value.ToList();
                    action(null, null);
                };
        }


		public static void LoadSummaries(EventHandler action)
		{
            // Hit the server each time for updated information based on fiscal year.
            if (Summaries.Count == 0 || !Summaries.Any(e=>e.FiscalYear == CurrentFiscalYear))
            {
                HomeContext.Load(HomeContext.GetHomeSummariesByYearQuery(CurrentFiscalYear)).Completed += action;
            }
			else
			    action(null, null);
		}

		public static EntitySet<Home> Homes
		{
			get { return HomeContext.Homes; }
		}

		public static void LoadHomesBySummaries(List<HomeSummary> summaries, EventHandler action)
		{
			var notFound = new List<HomeSummary>();
			var d = Homes.ToDictionary(e => e.PIN);
			foreach (HomeSummary s in summaries)
			{
				if (!d.ContainsKey(s.PIN))
					notFound.Add(s);
			}
			if (notFound.Count != 0)
			{
				string pins = String.Join(" ", notFound.Select(x => x.PIN));
				HomeContext.Load(HomeContext.GetHomesByPinsQuery(pins)).Completed += action;
			}
			else
				action(null, null);
		}


		public static EntitySet<Description> Descriptions
		{
			get { return HomeContext.Descriptions; }
		}

		public static EntitySet<FinanceSummary> FinanceSummaries
		{
			get { return HomeContext.FinanceSummaries; }
		}

        public static List<FinanceSummary> FinanceSummariesThisYear
        {
            get { return HomeContext.FinanceSummaries.Where(e=>e.FiscalYear == CurrentFiscalYear).ToList(); }
        }


		public static void LoadFinanceSummaries(EventHandler action)
		{
            // Hit the server each time for updated information based on fiscal year.
			if (FinanceSummaries.Count == 0 || !FinanceSummaries.Any(e => e.FiscalYear == CurrentFiscalYear))
			{
				HomeContext.Load(HomeContext.GetFinanceSummaryQuery().Where(x => x.FiscalYear == CurrentFiscalYear)).Completed += action;
			}
			else
				action(null, null);
		}

		public static EntitySet<Summary> Finances
		{
			get { return HomeContext.Summaries; }
		}

		public static void LoadFinancesBySummaries(List<HomeSummary> summaries, EventHandler action)
		{
			var notFound = new List<HomeSummary>();
			var d = Finances.Where(x => x.FiscalYear == CurrentFiscalYear).ToDictionary(e => e.PIN);
			foreach (HomeSummary s in summaries)
			{
				if (!d.ContainsKey(s.PIN))
					notFound.Add(s);
			}
			if (notFound.Count != 0)
			{
				string pins = String.Join(" ", notFound.Select(x => x.PIN));
				HomeContext.Load(HomeContext.GetFinancesByPinsQuery(pins, CurrentFiscalYear)).Completed += action;
			}
			else
				action(null, null);
		}


		public static EntitySet<CountyAverage> CountyAverages
		{
			get { return HomeContext.CountyAverages; }
		}

		public static EntitySet<StateAverage> StateAverages
		{
			get { return HomeContext.StateAverages; }
		}

        public static EventHandler HomeChanged;
        private static HomeSummary _currentSummary;
		public static HomeSummary CurrentSummary
		{
			get { return _currentSummary; }
			set
			{
				bool changed = _currentSummary == null && value != null || value != null && _currentSummary.PIN != value.PIN;
				_currentSummary = value;
				if (HomeChanged != null && changed)
					HomeChanged(null, null);
			}
		}

        public static EventHandler FiscalYearChanged;
        private static int _currentFiscalYear;// = int.Parse(ApplicationStrings.FiscalYear);
		public static int CurrentFiscalYear
		{
            get { return _currentFiscalYear; }
            set
            {
                bool changed = _currentFiscalYear != value;
                _currentFiscalYear = value;
                if (FiscalYearChanged != null && changed)
                    FiscalYearChanged(null, null);
            }
		}

		public static string CurrentState
		{
			get { return ApplicationStrings.StateCode; }
		}

		public static Description CurrentDescription
		{
			get { return Descriptions.FirstOrDefault(e => e.PIN == CurrentSummary.PIN && e.FiscalYear == CurrentFiscalYear); }
		}

		public static Home CurrentHome
		{
			get { return Homes.FirstOrDefault(e => e.PIN == CurrentSummary.PIN); }
		}

		public static Summary CurrentFinances
		{
			get { return Finances.FirstOrDefault(e => e.PIN == CurrentSummary.PIN && e.FiscalYear == CurrentFiscalYear); }
		}

		public static CountyAverage CurrentCountyAverage
		{
			get { return CountyAverages.FirstOrDefault(e => e.CountyCode == CurrentSummary.CountyCode && e.FiscalYear == CurrentFiscalYear); }
		}

		public static StateAverage CurrentStateAverage
		{
			get { return StateAverages.FirstOrDefault(e => e.State == CurrentState && e.FiscalYear == CurrentFiscalYear); }
		}

		static Data()
		{
			HomeContext = new HomeContext();
		}

		public static void Load<T>(T current, EventHandler action) where T : class
		{
			if (current != null)
			{
				action(null, null);
				return;
			}

			LoadOperation op;
			if (typeof(T) == typeof(Home))
				op = HomeContext.Load(HomeContext.GetHomesByPinQuery(CurrentSummary.PIN));
			else if (typeof(T) == typeof(Description))
                op = HomeContext.Load(HomeContext.GetDescriptionsQuery().Where(e => e.PIN == CurrentSummary.PIN && e.FiscalYear == CurrentFiscalYear));
			else if (typeof(T) == typeof(Summary))
				op = HomeContext.Load(HomeContext.GetFinancesQuery().Where(e => e.PIN == CurrentSummary.PIN && e.FiscalYear == CurrentFiscalYear));
			else if (typeof(T) == typeof(CountyAverage))
				op = HomeContext.Load(HomeContext.GetCountyAveragesQuery().Where(e => e.CountyCode == CurrentSummary.CountyCode && e.FiscalYear == CurrentFiscalYear));
			else if (typeof(T) == typeof(StateAverage))
				op = HomeContext.Load(HomeContext.GetStateAveragesQuery().Where(e => e.State == CurrentState && e.FiscalYear == CurrentFiscalYear));
			//else if (typeof (T) == typeof (EntitySet<HomeSummary>))
			//    NursingHomeContext.Load(NursingHomeContext.GetNursingHomeSummariesQuery()).Completed += action;
			else throw new ArgumentException("Unrecognized type given as parameter Current");

			op.Completed += (x, y) => action(x, y);
		}

	}

}
