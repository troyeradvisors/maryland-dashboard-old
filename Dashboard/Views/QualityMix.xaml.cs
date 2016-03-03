using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Navigation;
using Dashboard.Web;
using System.Linq;
using Dashboard.Web.Services;
using System.ServiceModel.DomainServices.Client;
using System;

namespace Dashboard
{

    public partial class QualityMix : Page
    {
		public HomeSummary LoadedSummary { get; set; }
		public Summary Finances { get { return Data.CurrentFinances; } }
		public CountyAverage CountyAverage { get { return Data.CurrentCountyAverage; } }
        public StateAverage StateAverage { get { return Data.CurrentStateAverage; } }

        public QualityMix()
        {
            InitializeComponent();
            this.Title = ApplicationStrings.DetailPageTitle;
        }
	
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Data.HomeChanged += Home_Changed;
			if (LoadedSummary != Data.CurrentSummary)
				Home_Changed(null, null);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			Data.HomeChanged -= Home_Changed;
			
		}

    	private void Home_Changed(object source, EventArgs e)
    	{
			Data.Load(Data.CurrentFinances, Finances_Loaded);
			Data.Load(Data.CurrentCountyAverage, County_Loaded);
			Data.Load(Data.CurrentStateAverage, State_Loaded);
			LoadedSummary = Data.CurrentSummary;
    	}

 
        void Finances_Loaded(object sender, System.EventArgs e)
        {
        	LayoutRoot.DataContext = Finances; 
            bool empty = Finances.ComprehensivePayor1DailyCensus + Finances.ComprehensivePayor2DailyCensus + Finances.ComprehensivePayor3DailyCensus == 0;
			(barRatePRD.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", Finances.PrivateRevenuePerDay, "Subject"), Tuple.Create("Medicare", Finances.MedicareRevenuePerDay, "Subject"), Tuple.Create("Medicaid", Finances.MedicaidRevenuePerDay, "Subject"), Tuple.Create("Other", Finances.OtherComprehensiveRevenuePerDay, "Subject"), Tuple.Create("Total", Finances.ComprehensiveRevenuePerDay, "Subject") };
			(barOtherRatePRD.Series[0] as ColumnSeries).Title = tbName1.Text = FixString(Finances.ComprehensivePayor1Name);
			(barOtherRatePRD.Series[1] as ColumnSeries).Title = tbName2.Text = FixString(Finances.ComprehensivePayor2Name);
			(barOtherRatePRD.Series[2] as ColumnSeries).Title = tbName3.Text = FixString(Finances.ComprehensivePayor3Name);
			(barOtherRatePRD.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.ComprehensivePayor1RevenuePerDay, tbName1.Text) };
			(barOtherRatePRD.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.ComprehensivePayor2RevenuePerDay, tbName2.Text) };
			(barOtherRatePRD.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.ComprehensivePayor3RevenuePerDay, tbName3.Text) };
			(barDailyCensus.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", Finances.PrivateDays / 365.0, "Subject"), Tuple.Create("Medicare", Finances.MedicareDays / 365.0, "Subject"), Tuple.Create("Medicaid", Finances.MedicaidDays / 365.0, "Subject"), Tuple.Create("Other", Finances.OtherComprehensiveDays / 365.0, "Subject"), Tuple.Create("Total", Finances.ComprehensiveDays / 365.0, "Subject")};
        	(pieOtherDailyCensus.Series[0] as PieSeries).ItemsSource = null; // Pie chart seems to not reset itemssource when assigning new one. Manually clearing.
        	var p = pieOtherDailyCensus.Palette;
        	pieOtherDailyCensus.Palette = null;
        	pieOtherDailyCensus.Palette = p;
			(pieOtherDailyCensus.Series[0] as PieSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create(tbName1.Text, empty ? 1 : Finances.ComprehensivePayor1DailyCensus), Tuple.Create(tbName2.Text, empty ? 1 : Finances.ComprehensivePayor2DailyCensus), Tuple.Create(tbName3.Text, empty ? 1 : Finances.ComprehensivePayor3DailyCensus) };
        	FixLayout();
        }

		string FixString(string s)
		{
			s = (s ?? "").Trim();
			if (s == "0")
				s = "";
			s = s == "" ? "-" : s;
			return s;
		}

        void County_Loaded(object sender, System.EventArgs e)
        {
			(barRatePRD.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", CountyAverage.PrivateRevenuePerDay, "County"), Tuple.Create("Medicare", CountyAverage.MedicareRevenuePerDay, "County"), Tuple.Create("Medicaid", CountyAverage.MedicaidRevenuePerDay, "County"), Tuple.Create("Other", CountyAverage.OtherComprehensiveRevenuePerDay, "County"), Tuple.Create("Total", CountyAverage.ComprehensiveRevenuePerDay, "County") };
			(barDailyCensus.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", CountyAverage.PrivateDays / 365.0, "County"), Tuple.Create("Medicare", CountyAverage.MedicareDays / 365.0, "County"), Tuple.Create("Medicaid", CountyAverage.MedicaidDays / 365.0, "County"), Tuple.Create("Other", CountyAverage.OtherComprehensiveDays / 365.0, "County"), Tuple.Create("Total", CountyAverage.ComprehensiveDays / 365.0, "County") };
        	FixLayout();
        }

        void State_Loaded(object sender, System.EventArgs e)
        {
			(barRatePRD.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", StateAverage.PrivateRevenuePerDay, "State"), Tuple.Create("Medicare", StateAverage.MedicareRevenuePerDay, "State"), Tuple.Create("Medicaid", StateAverage.MedicaidRevenuePerDay, "State"), Tuple.Create("Other", StateAverage.OtherComprehensiveRevenuePerDay, "State"), Tuple.Create("Total", StateAverage.ComprehensiveRevenuePerDay, "State") };
			(barDailyCensus.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", StateAverage.PrivateDays / 365.0, "State"), Tuple.Create("Medicare", StateAverage.MedicareDays / 365.0, "State"), Tuple.Create("Medicaid", StateAverage.MedicaidDays / 365.0, "State"), Tuple.Create("Other", StateAverage.OtherComprehensiveDays / 365.0, "State"), Tuple.Create("Total", StateAverage.ComprehensiveDays / 365.0, "State") };
        	FixLayout();
        }

   		private int loadCount = 0;
		void FixLayout()
		{
			if ((++loadCount) %3==0)
			gridOther.Width = gridSubject.Width = Math.Max(gridOther.Width, gridSubject.Width);
		}

		private void This_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			ContentStack.MaxWidth = e.NewSize.Width;
		}

    }
}