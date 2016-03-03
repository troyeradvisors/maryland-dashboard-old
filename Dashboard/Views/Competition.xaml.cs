using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
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
using System.Windows.Controls.DataVisualization;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl;


namespace Dashboard
{
	public partial class Competition : Page, ICustomPrint
	{
		#region Fields/Properties

		private ObservableCollection<HomeSummary> _selectedSummaries = new ObservableCollection<HomeSummary>();
		public ObservableCollection<HomeSummary> SelectedSummaries
		{
			get { return _selectedSummaries; }
			set { _selectedSummaries = value; }
		}
		private ObservableCollection<HomeSummary> _unselectedSummaries = new ObservableCollection<HomeSummary>();
		public ObservableCollection<HomeSummary> UnselectedSummaries
		{
			get { return _unselectedSummaries; }
			set { _unselectedSummaries = value; }
		}

		private bool IsPrinting = false;
		private HomeSummary LoadedSummary { get; set; }
        private int LoadedFiscalYear { get; set; } 
		public Home AverageHome;
		public Summary AverageFinances;

		private int SeriesCount
		{
			// + 1 to account for average being added in.
			get { return (SelectedSummaries.Count == 1 ? 1 : SelectedSummaries.Count + 1); }
		}

		private enum ChartType { Bar, Column }
		private ResourceDictionaryCollection OriginalColumnPalette, CurrentColumnPalette, OriginalBarPalette, CurrentBarPalette;
		private List<Home> SelectedHomes = new List<Home>();
		private List<Summary> SelectedFinances = new List<Summary>(); 

		#endregion

		#region Constructors/Loading

		public Competition()
		{
			InitializeComponent();
			this.Title = ApplicationStrings.CompetitionPageTitle;
			Data.LoadSummaries((x, y) =>
			{
			    foreach (var home in Data.SummariesThisYear)
			        UnselectedSummaries.Add(home);
			});

		}

		private void This_Loaded(object sender, RoutedEventArgs e)
		{
			legendMargin = new Thickness(300, 200, 0, 0);
			UpdateLegendWindowPosition();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
            Data.FiscalYearChanged += FiscalYear_Changed;
            Data.HomeChanged += Home_Changed;
            if (LoadedFiscalYear != Data.CurrentFiscalYear)
                FiscalYear_Changed(null, null);
			else if (LoadedSummary != Data.CurrentSummary)
				Home_Changed(null, null);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			Data.HomeChanged -= Home_Changed;
            Data.FiscalYearChanged -= FiscalYear_Changed;
        }

		private void Home_Changed(object source, EventArgs e)
		{
            // Ignore updating home because a fiscal update is currently in progress.
            if (LoadedFiscalYear != Data.CurrentFiscalYear)
                return;
			UnselectedSummaries.Remove(Data.CurrentSummary);
			lbUnselected.ItemsSource = UnselectedSummaries;
			SelectedSummaries.Insert(0, Data.CurrentSummary);
			lbSelected.ItemsSource = null;
			lbSelected.ItemsSource = SelectedSummaries;
			if (LoadedSummary != null)
				Move(Direction.Left, new List<HomeSummary>() { LoadedSummary }, autoLoadData: false);

			LoadedSummary = Data.CurrentSummary;
			LoadData();
		}
        private void FiscalYear_Changed(object source, EventArgs e)
        {

            Data.LoadSummaries((x, y) =>
            {
                LoadedFiscalYear = Data.CurrentFiscalYear;

                //List<HomeSummary> old = new List<HomeSummary>();
                //old.AddRange(SelectedSummaries);
                UnselectedSummaries.Clear();
                SelectedSummaries.Clear();
                foreach (var home in Data.SummariesThisYear)
                    UnselectedSummaries.Add(home);
                //Move(Direction.Right, UnselectedSummaries.Where(a => old.Any(b => a.PIN == b.PIN)).ToList(), false);
                //SelectedSummaries.Remove(Data.CurrentSummary); // Home_changed will take care of moving current over to the right.
                Home_Changed(null, null);
            });
        }
	
		


		private int DataLoadedCount;
		private void LoadData()
		{
			UpdatePalette();
			var l = SelectedSummaries.ToList();
			Data.LoadHomesBySummaries(l, Home_Loaded);
			Data.LoadFinancesBySummaries(l, Finances_Loaded);
		}

		private void Home_Loaded(object sender, System.EventArgs e)
		{
			SelectedHomes.Clear();
			List<Home> otherHomes = new List<Home>();
			var dHomes = Data.Homes.ToDictionary(x => x.PIN);
			foreach (HomeSummary s in SelectedSummaries)
			{
				Home h = dHomes[s.PIN];
				SelectedHomes.Add(h);
				if (s.PIN != Data.CurrentSummary.PIN)
					otherHomes.Add(h);
			}
			
			AverageHome = CalculateAverageHome(otherHomes);
			UpdateMap(SelectedHomes);
			UpdatePage();
		}

		private void Finances_Loaded(object sender, EventArgs e)
		{
			SelectedFinances.Clear();
			List<Summary> otherFinances = new List<Summary>();
			var dFinances = Data.Finances.Where(f=>f.FiscalYear == Data.CurrentFiscalYear).ToDictionary(x => x.PIN);
			foreach (HomeSummary s in SelectedSummaries)
			{
				var f = dFinances[s.PIN];								
				SelectedFinances.Add(f);
				if (s.PIN != Data.CurrentSummary.PIN)
					otherFinances.Add(f);
			}
			
			AverageFinances = CalculateAverageFinances(otherFinances);
			UpdateCharts(SelectedFinances);
			UpdatePage();
		}

		private void UpdatePage()
		{
			++DataLoadedCount;
			if (DataLoadedCount%2 == 0)
			{
				UpdateGrid(SelectedHomes, SelectedFinances);
			}

		}

		private void UpdatePalette()
		{
			if (colAveragePayorMix.Palette.Count == 11)
			{
				OriginalColumnPalette = colAveragePayorMix.Palette as ResourceDictionaryCollection;
				OriginalBarPalette = barFacilityMedicaidRate.Palette as ResourceDictionaryCollection;
			}
			if (SeriesCount <= 11)
			{
				CurrentColumnPalette = OriginalColumnPalette;
				CurrentBarPalette = OriginalBarPalette;
			}
			else
			{
				CurrentBarPalette = GeneratePalette(SeriesCount, ChartType.Bar);
				CurrentColumnPalette = GeneratePalette(SeriesCount, ChartType.Column);
			}
		}


		private void UpdateMap(List<Home> homes)
		{
			map.Children.Clear();
			MapLayer layer = new MapLayer();
			map.Children.Add(layer);	
			List<Location> locations = new List<Location>();
			List<Color> colors = CurrentColumnPalette.Select(e => (((e["DataPointStyle"] as Style).Setters.First(x=>(x as Setter).Property==Control.BackgroundProperty) as Setter).Value as RadialGradientBrush).GradientStops[1].Color).ToList();
			int i = 0;
			foreach (var home in homes)
			{
				Location l = new Location(home.Latitude, home.Longitude);
				Pushpin pin = new Pushpin();
				ToolTipService.SetToolTip(pin, new ToolTip() { Content = home.Name });
				pin.Background = new SolidColorBrush(colors[i==0? 0 : i+1]);
				pin.Location = l;
				layer.AddChild(pin, pin.Location);
				locations.Add(l);
				++i;
			}
			if (locations.Count == 1)
				map.SetView(locations[0], 10);
			else
				map.SetView(new LocationRect(locations));

		}

		private Home CalculateAverageHome(List<Home> homes)
		{
			Home home = new Home();
			if (homes.Count > 0)
			{
				home.Name = "Average (" + homes.Count + ")";
			}
			return home;
		}

		private Summary CalculateAverageFinances(List<Summary> finances)
		{
			//var subset = data;
			Summary finance = new Summary();
			if (finances.Count > 0)
			{
				finance.CertifiedBedCount = (int)Math.Round(finances.Average(e => e.CertifiedBedCount));
				finance.PrivateDaysPercent = finances.Average(e => e.PrivateDaysPercent);
				finance.MedicareDaysPercent = finances.Average(e => e.MedicareDaysPercent);
				finance.MedicaidDaysPercent = finances.Average(e => e.MedicaidDaysPercent);
				finance.OtherComprehensiveDaysPercent = finances.Average(e => e.OtherComprehensiveDaysPercent);
				finance.NursingOccupancyPercent = finances.Average(e => e.NursingOccupancyPercent);
				finance.PrivateRevenuePerDay = finances.Average(e => e.PrivateRevenuePerDay);
				finance.MedicareRevenuePerDay = finances.Average(e => e.MedicareRevenuePerDay);
				finance.MedicaidRevenuePerDay = finances.Average(e => e.MedicaidRevenuePerDay);
				finance.OtherComprehensiveRevenuePerDay = finances.Average(e => e.OtherComprehensiveRevenuePerDay);
				finance.PrivateDailyCensus = finances.Average(e => e.PrivateDailyCensus);
				finance.MedicareDailyCensus = finances.Average(e => e.MedicareDailyCensus);
				finance.MedicaidDailyCensus = finances.Average(e => e.MedicaidDailyCensus);
				finance.OtherComprehensiveDailyCensus = finances.Average(e => e.OtherComprehensiveDailyCensus);
				finance.TotalRevenue = (int) Math.Round(finances.Average(e => e.TotalRevenue));
				finance.TotalExpense = finances.Average(e => e.TotalExpense);
				finance.NetIncome = finances.Average(e => e.NetIncome);
				finance.NetIncomePerRevenuePercent = finances.Average(e => e.NetIncomePerRevenuePercent);
			}
			return finance;
		}
		#endregion	

		#region Chart Updates

		private void ResetCharts()
		{
			foreach (Chart chart in wrapChart.Descendants<Chart>().Union(new List<Chart>(){legend}))
			{
				chart.Series.Clear();
				chart.Palette = null;
				if (chart.Name.ToLower().StartsWith("col") || chart == legend)
					chart.Palette = CurrentColumnPalette;
				if (chart.Name.ToLower().StartsWith("bar"))
					chart.Palette = CurrentBarPalette;
				if (chart != legend)
					chart.LegendStyle = App.Current.Resources["NoLegendStyle"] as Style;
			}

			double max = double.NegativeInfinity;
			foreach (Chart chart in new List<Chart>() { colAveragePayorMix, colAverageRatePRD, colAverageDailyCensus, colNetOperatingIncome })
			{
				//while (chart.Series.Count > SelectedSummaries.Count)  // dont forget about clearing average column!
				//    chart.Series.RemoveAt(chart.Series.Count-1);
				//while (chart.Series.Count < SelectedSummaries.Count)
				//    chart.Series.Add(new ColumnSeries() { Title = seriesTitle, IndependentValuePath = "Item1", DependentValuePath = "Item2" });
				int barsCount = (chart != colNetOperatingIncome ? 4 : 3)*SeriesCount;
				chart.MinWidth = 40*barsCount * Math.Pow(.95, barsCount) + 10 * barsCount;
				chart.Height = 200;
				max = Math.Max(max, chart.MinWidth);
			}
			WrapPanelSize = max;
		}

		private void UpdateCharts(List<Summary> finances )
		{
			ResetCharts();
			Summary finance;
			string[] empty = new string[] { string.Empty };
			string[] payorTypes = { "Private", "Medicare", "Medicaid", "Other" };
			bool addedAverage = false;
			for (int i = 0; i < SelectedSummaries.Count; ++i)
			{
				if (SelectedSummaries.Count > 1 && !addedAverage && i == 1)
				{
					finance = AverageFinances;
					addedAverage = true;
					--i;
				}
				else
					finance = finances[i];

				string title = finance == AverageFinances ? "Average" : SelectedSummaries[i].Name;

				UpdateChart(legend, title, new string[] { "" }, new object[] { 0 }, ChartType.Column);
				UpdateChart(colAveragePayorMix, title, payorTypes, new object[] { finance.PrivateDaysPercent, finance.MedicareDaysPercent, finance.MedicaidDaysPercent, finance.OtherComprehensiveDaysPercent }, ChartType.Column);
				UpdateChart(colAverageRatePRD, title, payorTypes, new object[] { finance.PrivateRevenuePerDay, finance.MedicareRevenuePerDay, finance.MedicaidRevenuePerDay, finance.OtherComprehensiveRevenuePerDay }, ChartType.Column);
				UpdateChart(colAverageDailyCensus, title, payorTypes, new object[] { finance.PrivateDailyCensus, finance.MedicareDailyCensus, finance.MedicaidDailyCensus, finance.OtherComprehensiveDailyCensus }, ChartType.Column);
				UpdateChart(colNetOperatingIncome, title, new string[] { "Revenue", "Expenses", "Income" }, new object[] { (decimal)finance.TotalRevenue, finance.TotalExpense, finance.NetIncome}, ChartType.Column); //finance.TotalRevenue , finance.TotalExpense, finance.NetIncome

				if (finance.PIN == Data.CurrentSummary.PIN || finance == AverageFinances)
				{
					UpdateChart(barFacilityOccupancy, title, empty, new object[] { finance.NursingOccupancyPercent }, ChartType.Bar);
					UpdateChart(barFacilityMedicareRate, title, empty, new object[] { finance.MedicareRevenuePerDay }, ChartType.Bar);
					UpdateChart(barFacilityMedicaidRate, title, empty, new object[] { finance.MedicaidRevenuePerDay }, ChartType.Bar);
					UpdateChart(barFacilityProfitMargin, title, empty, new object[] { finance.NetIncomePerRevenuePercent }, ChartType.Bar);
					UpdateChart(barFacilityMedicareMix, title, empty, new object[] { finance.MedicareDaysPercent }, ChartType.Bar);
				}
			}


		}

		private void UpdateChart(Chart chart, string seriesTitle, string[] titles, object[] values, ChartType chartType)
		{
            System.Diagnostics.Debug.Assert(values.Select(e => e.GetType().ToString()).Distinct().Count() == 1, "All values in object must be same type for chart to work!");

			ObservableObjectCollection data = new ObservableObjectCollection();
			for (int i = 0; i < titles.Count(); ++i)
				data.Add(Tuple.Create(titles[i], values[i], seriesTitle));
			if (chartType == ChartType.Column)
			{
				chart.Series.Add(new ColumnSeries() { Title = seriesTitle, IndependentValuePath = "Item1", DependentValuePath = "Item2" });
				(chart.Series.Last() as ColumnSeries).ItemsSource = data;
			}
			if (chartType == ChartType.Bar)
			{
				chart.Series.Add(new BarSeries() { Title = seriesTitle, IndependentValuePath = "Item1", DependentValuePath = "Item2" });
				(chart.Series.Last() as BarSeries).ItemsSource = data;
			}
		}



		#endregion

		#region Grid Updates

		private void UpdateGrid(List<Home> homes, List<Summary> finances  )
		{
			foreach (UIElement child in grid.Children.ToList())
				grid.Children.Remove(child);

			int neededColumns = SelectedSummaries.Count + (SelectedSummaries.Count > 1 ? 1 : 0);
			while (grid.ColumnDefinitions.Count > neededColumns)
				grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
			while (grid.ColumnDefinitions.Count < neededColumns)
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(95) });


			for (int i = 0, col = 0; i < SelectedSummaries.Count; ++i)
			{
				PopulateColumn(col++, homes[i], finances[i]);
				if (i == 0 && SelectedSummaries.Count > 1)
					PopulateColumn(col++, AverageHome, AverageFinances);
			}

			grid.Children.Remove(Splitter);
			grid.Children.Add(Splitter); // Send it to the foreground.
            if (grid.ColumnDefinitions.Count > 0)
			    Grid.SetColumnSpan(Splitter, grid.ColumnDefinitions.Count);

			for (int i = 0; i < grid.RowDefinitions.Count; ++i)  // Reset minHeight so calling actualHeight won't take into account the old minHeight and potentially be too large.
				gridHeader.RowDefinitions[i].MinHeight = grid.RowDefinitions[i].MinHeight = 0;

			gridHeader.UpdateLayout();
			grid.UpdateLayout();

			for (int i = 0; i < grid.RowDefinitions.Count; ++i)
			{
				gridHeader.RowDefinitions[i].MinHeight = grid.RowDefinitions[i].MinHeight = (Math.Max(gridHeader.RowDefinitions[i].ActualHeight, grid.RowDefinitions[i].ActualHeight));
			}

			grid.UpdateLayout();

			UpdateMapSize();
			UpdateWrapSize();

		}



		private void PopulateColumn(int col, Home home, Summary finances)
		{
			AddTextBlock(1, col, home.Name, "GridValueHeaderStyle", true, VerticalAlignment.Top, 5);
			AddTextBlock(2, col, home.Street, "GridValueStyle", true, VerticalAlignment.Top);
			AddTextBlock(3, col, home.City + (string.IsNullOrEmpty(home.City) ? "" : ", ") + home.State, "GridValueStyle", true);
			AddTextBlock(4, col, finances.CertifiedBedCount.ToString(), "GridValueStyle");
			AddTextBlock(7, col, String.Format("{0:0}%", finances.PrivateDaysPercent), "GridValueStyle");
			AddTextBlock(8, col, String.Format("{0:0}%", finances.MedicareDaysPercent), "GridValueStyle");
			AddTextBlock(9, col, String.Format("{0:0}%", finances.MedicaidDaysPercent), "GridValueStyle");
			AddTextBlock(10, col, String.Format("{0:0}%", finances.OtherComprehensiveDaysPercent), "GridValueStyle");
			AddTextBlock(11, col, String.Format("{0:0}%", finances.NursingOccupancyPercent), "GridValueHeaderStyle");

			AddTextBlock(14, col, String.Format("{0:C}", finances.PrivateRevenuePerDay), "GridValueStyle");
			AddTextBlock(15, col, String.Format("{0:C}", finances.MedicareRevenuePerDay), "GridValueStyle");
			AddTextBlock(16, col, String.Format("{0:C}", finances.MedicaidRevenuePerDay), "GridValueStyle");
			AddTextBlock(17, col, String.Format("{0:C}", finances.OtherComprehensiveRevenuePerDay), "GridValueStyle");

			AddTextBlock(20, col, String.Format("{0:N1}", finances.PrivateDailyCensus), "GridValueStyle");
			AddTextBlock(21, col, String.Format("{0:N1}", finances.MedicareDailyCensus), "GridValueStyle");
			AddTextBlock(22, col, String.Format("{0:N1}", finances.MedicaidDailyCensus), "GridValueStyle");
			AddTextBlock(23, col, String.Format("{0:N1}", finances.OtherComprehensiveDailyCensus), "GridValueStyle");

			AddTextBlock(25, col, String.Format("{0:C0}", finances.TotalRevenue), "GridValueStyle");
			AddTextBlock(26, col, String.Format("{0:C0}", finances.TotalExpense), "GridValueStyle");
			AddTextBlock(27, col, String.Format("{0:C0}", finances.NetIncome), "GridValueHeaderStyle");
			AddTextBlock(28, col, String.Format("{0:0}%", finances.NetIncomePerRevenuePercent), "GridValueHeaderStyle");
		}

		private TextBlock AddTextBlock(int row, int column, string text, string style, bool smallText = false, VerticalAlignment vert=VerticalAlignment.Bottom, double bottomMargin=0)
		{
			TextBlock tb = new TextBlock() { Text = text, Style = App.Current.Resources[style] as Style, VerticalAlignment = vert, Margin=new Thickness(1,0,1,bottomMargin) };
			Grid.SetColumn(tb, column);
			Grid.SetRow(tb, row);
			if (smallText) tb.FontSize = (int) Math.Round((double) App.Current.Resources["ContentFontSize"]*.65);
			grid.Children.Add(tb);
			return tb;
		} 
		#endregion

		#region Helpers

		private Thickness legendMargin;
		private void UpdateLegendWindowPosition()
		{
			double currentTop = Canvas.GetTop(legendWindow);
			double currentLeft = Canvas.GetLeft(legendWindow);
			Thickness currentMargin = new Thickness(currentLeft, currentTop, this.ActualWidth - currentLeft, this.ActualHeight - currentTop);
			Canvas.SetLeft(legendWindow, currentLeft - (legendMargin.Left - currentMargin.Right));
			Canvas.SetTop(legendWindow, currentTop - (legendMargin.Top - currentMargin.Bottom));
		}

		private double WrapPanelSize = double.NegativeInfinity; 
		private void UpdateWrapSize()
		{
			wrapChart.MaxWidth = new double[] { WrapPanelSize, this.ActualWidth }.Max();
			//wrapChart.MaxWidth = new double[] { WrapPanelSize, this.ActualWidth, gridScrollViewer.ActualWidth }.Max();
		}

		private void UpdateMapSize()
		{
			map.Width = Math.Max(0, this.ActualWidth - stackGrids.ActualWidth - 100);
			if (map.Width >= map.MinWidth)
			{
				map.Height = grid.ActualHeight - 20;
				map.Margin = new Thickness(40, 10, 0, 0);
			}
			else
			{
				map.Width = this.ActualWidth - 80;
				map.Height = 0; // Reset to min height
				map.Margin = new Thickness(0, 10, 0, 20);
			}
		}

		private ResourceDictionaryCollection GeneratePalette(int colorCount, ChartType chart)
		{
			var palette = new ResourceDictionaryCollection();
			for (int i = 0; i < colorCount; ++i)
			{
				RadialGradientBrush gradient = new RadialGradientBrush() { Center = new Point(.075, .015), GradientOrigin = new Point(-.1, -.1), RadiusY = .9, RadiusX = 1.05 };
				int hue = (i * 360 / colorCount) % 360;
				double brightness = .95;
				double saturation = .99;
				Color light = FromHSV(hue, saturation, brightness+.02);
				Color dark = FromHSV(hue, saturation, brightness);
				gradient.GradientStops.Add(new GradientStop() { Color = light });
				gradient.GradientStops.Add(new GradientStop() { Color = dark, Offset = 1.0 });
				Style pointStyle = new Style() { TargetType = typeof(Control) };
				pointStyle.Setters.Add(new Setter() { Property = Control.BackgroundProperty, Value = gradient });
				if (chart == ChartType.Column)
					pointStyle.Setters.Add(new Setter() { Property = Control.TemplateProperty, Value = App.Current.Resources["MyColumnDataPointTemplate"] });
				else
					pointStyle.Setters.Add(new Setter() { Property = Control.TemplateProperty, Value = App.Current.Resources["MyBarDataPointTemplate"] });

				Style shapeStyle = new Style() { TargetType = typeof(Shape) };
				shapeStyle.Setters.Add(new Setter() { Property = Shape.StrokeProperty, Value = gradient });
				shapeStyle.Setters.Add(new Setter() { Property = Shape.StrokeThicknessProperty, Value = 2 });
				shapeStyle.Setters.Add(new Setter() { Property = Shape.StrokeMiterLimitProperty, Value = 1 });
				shapeStyle.Setters.Add(new Setter() { Property = Shape.FillProperty, Value = gradient });
				ResourceDictionary rd = new ResourceDictionary();
				rd.Add("DataPointStyle", pointStyle);
				rd.Add("DataShapeStyle", shapeStyle);
				palette.Add(rd);
			}
			return palette;
		}

		/// <param name="hue">Hue from 0..360 degrees.</param>
		/// <param name="saturation">Saturation from 0..1</param>
		/// <param name="value">Brightness or value from 0..1</param>
		private Color FromHSV(double hue, double saturation, double value)
		{
			int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
			double f = hue / 60 - Math.Floor(hue / 60);

			value = value * 255;
			var v = Convert.ToByte(value);
			var p = Convert.ToByte(value * (1 - saturation));
			var q = Convert.ToByte(value * (1 - f * saturation));
			var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

			if (hi == 0)
				return Color.FromArgb(255, v, t, p);
			else if (hi == 1)
				return Color.FromArgb(255, q, v, p);
			else if (hi == 2)
				return Color.FromArgb(255, p, v, t);
			else if (hi == 3)
				return Color.FromArgb(255, p, q, v);
			else if (hi == 4)
				return Color.FromArgb(255, t, p, v);
			else
				return Color.FromArgb(255, v, p, q);
		}

		private enum Direction
		{
			Up,
			Left,
			Down,
			Right
		}

		private void Move(Direction direction, List<HomeSummary> summaries, bool autoLoadData = true, bool userRequest=false)
		{
			bool doNotMoveCurrent = userRequest && direction == Direction.Left;
			bool dataMoved = false;
			foreach (HomeSummary summary in summaries)
			{
				if (summary == Data.CurrentSummary && doNotMoveCurrent) continue;
				if (direction == Direction.Right)
				{
                    if (UnselectedSummaries.Remove(summary))
                    {
                        SelectedSummaries.Add(summary);
                        dataMoved = true;
                    }
				}
				if (direction == Direction.Left)
				{
                    if (SelectedSummaries.Remove(summary))
                    {
                        UnselectedSummaries.Add(summary);
                        dataMoved = true;
                    }
				}
			}
			if (dataMoved)
			{
				lbSelected.ItemsSource = null;
				lbSelected.ItemsSource = SelectedSummaries;
				if (direction == Direction.Left)
				{
					var ordered = UnselectedSummaries.OrderBy(e => e.Name).ToList();
					UnselectedSummaries.Clear();
					foreach (var s in ordered)
						UnselectedSummaries.Add(s);
				}
				lbUnselected.ItemsSource = UnselectedSummaries;
				if (autoLoadData)
					LoadData();
			}
		}

		#endregion

		#region Events

		private void btnMoveRight_Click(object sender, RoutedEventArgs e)
		{
			Move(Direction.Right, lbUnselected.SelectedItems.OfType<HomeSummary>().ToList(), userRequest:true );
		}

		private void btnMoveLeft_Click(object sender, RoutedEventArgs e)
		{
			Move(Direction.Left, lbSelected.SelectedItems.OfType<HomeSummary>().ToList(), userRequest: true);
		}

		private void btnClear_Click(object sender, RoutedEventArgs e)
		{
			Move(Direction.Left, lbSelected.Items.OfType<HomeSummary>().ToList(), userRequest: true);
		}

		private void This_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			stackGrids.MaxWidth = this.ActualWidth - 23;
			//gridScrollViewer.MaxWidth = this.ActualWidth - 23;
			UpdateMapSize();
			UpdateWrapSize();
			UpdateLegendWindowPosition();
		}

		#endregion

		#region Printer

		ScrollViewer legendScroll;
		public void PrePrint(Size page)
		{
			IsPrinting = true;
			legendScroll = legendWindow.Content as ScrollViewer;
			legendWindow.Content = null;
			wrapChart.Children.Add(legendScroll);
			legendWindow.Visibility = Visibility.Collapsed;
			wrapChart.MaxWidth = page.Width / Printer.MIN_SCALE * .9;
			map.Width = map.Height = 0; // reset to minwidth
		}

		public void PostPrint()
		{
			wrapChart.Children.Remove(legendScroll);
			legendWindow.Content = legendScroll;
			legendWindow.Visibility = Visibility.Visible;
			IsPrinting = false;
		}

		public bool WantPrint
		{
			get
			{
				if (SelectedSummaries.Count > 13)
					ErrorWindow.CreateMessageBox("Printing more than 13 nursing homes is not supported.", "Caution");
				return true;
			}
		}

		#endregion
	}
}
