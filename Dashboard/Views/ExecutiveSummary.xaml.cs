using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using Dashboard.Controls;
using System.Windows.Navigation;
using System.Collections.Generic;
using Dashboard.Web;
using System.Linq;
using Dashboard.Web.Services;
using System.ServiceModel.DomainServices.Client;
using System;
using Microsoft.Maps.MapControl;
using Microsoft.Maps;
using System.Windows;

namespace Dashboard
{


    /// <summary>
    /// Home page for the application.
    /// </summary>
    public partial class ExecutiveSummary : Page
    {
		public HomeSummary LoadedSummary { get; set; }
		public Summary Finances { get { return Data.CurrentFinances; } }
        public CountyAverage CountyAverage { get { return Data.CurrentCountyAverage; } }
		public StateAverage StateAverage { get { return Data.CurrentStateAverage; } }

        /// <summary>
        /// Creates a new <see cref="Home"/> instance.
        /// </summary>
		public ExecutiveSummary()
        {
            InitializeComponent();
            this.Title = ApplicationStrings.SummaryPageTitle;
			BoundPalette(piePercentOfRevenue, 8);
			BoundPalette(pieSubjectPayorMix, 4);
			BoundPalette(pieCountyPayorMix, 4);
			BoundPalette(pieStatePayorMix, 4);
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
			Data.Load(Data.CurrentHome, Home_Loaded);
			LoadedSummary = Data.CurrentSummary;
    	}

		void Home_Loaded(object sender, System.EventArgs e)
		{
			map.Children.Clear();
			Location l = new Location(Data.CurrentHome.Latitude, Data.CurrentHome.Longitude);
			map.SetView(l, 10);
			Pushpin pin = new Pushpin();
			pin.Background = new SolidColorBrush(Color.FromArgb(255, 76, 123, 0));
			pin.Location = l;
			ToolTipService.SetToolTip(pin, new ToolTip() { Content = Data.CurrentHome.Name });
			MapLayer layer = new MapLayer();
			layer.AddChild(pin, pin.Location);
			map.Children.Add(layer);
		}

        void Finances_Loaded(object sender, System.EventArgs e)
        {
        	LayoutRoot.DataContext = Finances;
			(barSkilledNursingOccupancy.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.NursingOccupancyPercent, "Subject") };
			(barRevenuePerResidentDay.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.TotalRevenuePerDay, "Subject") };
			(barProfitMargin.Series[0] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", Finances.NetIncomePerRevenuePercent, "Subject") };
        	(piePercentOfRevenue.Series[0] as PieSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("G&A", Finances.GovernmentAndAdministrativeExpense), Tuple.Create("Dietary", Finances.DietaryExpense), Tuple.Create("H&L", Finances.HousekeepingAndLaundryExpense ), Tuple.Create("Care", Finances.NursingCareExpense), Tuple.Create("Other", Finances.OtherPatientCareExpense), Tuple.Create("Property", Finances.PropertyExpense ), Tuple.Create("Real Estate Tax", Finances.RealEstateTax ), Tuple.Create("Margin", Finances.NetIncome ) };
			(pieSubjectPayorMix.Series[0] as PieSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", Finances.PrivateDays), Tuple.Create("Medicare", Finances.MedicareDays), Tuple.Create("Medicaid", Finances.MedicaidDays), Tuple.Create("Other", Finances.OtherComprehensiveDays) };

        	tbImpliedValue.Text = String.Format("{0:C0}", Data.CurrentFinances.ImpliedMinValue) + " - " + String.Format("{0:C0}", Data.CurrentFinances.ImpliedMaxValue);
			tbImpliedValuePerBed.Text = String.Format("{0:C0}", Data.CurrentFinances.ImpliedMinValuePerBed) + " - " + String.Format("{0:C0}", Data.CurrentFinances.ImpliedMaxValuePerBed);
        
		}

    	private void BoundPalette(Chart c, int bound)
    	{
			var x = new Collection<ResourceDictionary>();
    		for (int i = 0; i < bound; ++i)
    			x.Add(c.Palette[i]);
    		c.Palette = x;
    		while (c.Palette.Count > 8)
    			c.Palette.RemoveAt(c.Palette.Count - 1);
    	}

    	void County_Loaded(object sender, System.EventArgs e)
        {
			(barSkilledNursingOccupancy.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", CountyAverage.NursingOccupancyPercent, "County") };
			(barRevenuePerResidentDay.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", CountyAverage.TotalRevenuePerDay, "County") };
			(barProfitMargin.Series[1] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", CountyAverage.NetIncomePerRevenuePercent, "County") };
            (pieCountyPayorMix.Series[0] as PieSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", CountyAverage.PrivateDays), Tuple.Create("Medicare", CountyAverage.MedicareDays), Tuple.Create("Medicaid", CountyAverage.MedicaidDays), Tuple.Create("Other", CountyAverage.OtherComprehensiveDays) };
		}

        void State_Loaded(object sender, System.EventArgs e)
        {
            tbComprehensiveRevenuePerDay.Text = String.Format("{0:C}", StateAverage.ComprehensiveRevenuePerDay);
            tbOtherRevenuePerDay.Text = String.Format("{0:C}", StateAverage.OtherRevenuePerDay);
            tbTotalRevenuePerDay.Text = String.Format("{0:C}", StateAverage.TotalRevenuePerDay);
            tbGovernmentAndAdministrationExpensePerDay.Text = String.Format("{0:C}", StateAverage.GovernmentAndAdministrativeExpensePerDay);
            tbDietaryExpensePerDay.Text = String.Format("{0:C}", StateAverage.DietaryExpensePerDay);
            tbHousekeepingAndLaundryExpensePerDay.Text = String.Format("{0:C}", StateAverage.HousekeepingAndLaundryExpensePerDay);
            tbNursingCareExpensePerDay.Text = String.Format("{0:C}", StateAverage.NursingCareExpensePerDay);
            tbOtherPatientCareExpensePerDay.Text = String.Format("{0:C}", StateAverage.OtherPatientCareExpensePerDay);
            tbPropertyExpensePerDay.Text = String.Format("{0:C}", StateAverage.PropertyExpensePerDay);
            tbRealEstateTaxPerDay.Text = String.Format("{0:C}", StateAverage.RealEstateTaxPerDay);
            tbTotalExpensePerDay.Text = String.Format("{0:C}", StateAverage.TotalExpensePerDay);
            tbNetIncomePerDay.Text = String.Format("{0:C}", StateAverage.NetIncomePerDay);

			(barSkilledNursingOccupancy.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", StateAverage.NursingOccupancyPercent, "State") };
			(barRevenuePerResidentDay.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", StateAverage.TotalRevenuePerDay, "State") };
			(barProfitMargin.Series[2] as ColumnSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("", StateAverage.NetIncomePerRevenuePercent, "State") };
            (pieStatePayorMix.Series[0] as PieSeries).ItemsSource = new ObservableObjectCollection() { Tuple.Create("Private", StateAverage.PrivateDays), Tuple.Create("Medicare", StateAverage.MedicareDays), Tuple.Create("Medicaid", StateAverage.MedicaidDays ), Tuple.Create("Other", StateAverage.OtherComprehensiveDays) };
		}


	}
}