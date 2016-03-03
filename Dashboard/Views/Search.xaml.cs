using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Security;
using Dashboard.Web;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Dashboard
{
	public partial class Search : Page
	{
        private int LoadedFiscalYear { get; set; } 

        private List<FinanceSummary> FinanceSummaries;
		public Search()
		{
			InitializeComponent();
			this.Title = ApplicationStrings.SearchPageTitle;
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Data.FiscalYearChanged += FiscalYear_Changed;
            if (LoadedFiscalYear != Data.CurrentFiscalYear)
                FiscalYear_Changed(null, null);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Data.FiscalYearChanged -= FiscalYear_Changed;
        }

        private void FiscalYear_Changed(object source, EventArgs e)
        {
            LoadFinanceSummaries();
        }

        private void LoadFinanceSummaries()
        {
            Data.LoadFinanceSummaries((x, y) => 
                {
                    LoadedFiscalYear = Data.CurrentFiscalYear;
                    FinanceSummaries = Data.FinanceSummariesThisYear;
                    if (financeSummaryDataGrid.ItemsSource == null)
                        financeSummaryDataGrid.ItemsSource = new PagedCollectionView(FinanceSummaries);
                    else
                    {
                        PagedCollectionView view = (financeSummaryDataGrid.ItemsSource as PagedCollectionView);
                        PagedCollectionView newView = new PagedCollectionView(FinanceSummaries);
                        financeSummaryDataGrid.ItemsSource = newView;
                        view.SortDescriptions.ToList().ForEach(z => newView.SortDescriptions.Add(z));
                        if (tbSearch.Text.Trim() != string.Empty)
                            tbSearch_TextChanged(null, null);
                    }
                });
        }


		private void btnClear_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			ResetSearch();
		}

		private void Page_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape) ResetSearch();
		}

		private void ResetSearch()
		{
			tbSearch.Text = "";
		}
		DateTime last = DateTime.MinValue;
		bool flag = false;
		Thread t;
		ManualResetEvent shouldStop = new ManualResetEvent(false);
		object locker = new object();
		private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = tbSearch.Text.ToLower().Trim();
			PagedCollectionView view = (financeSummaryDataGrid.ItemsSource as PagedCollectionView);
			shouldStop.Set();
			t = (new Thread(new ThreadStart(() =>
				 {
					 lock (locker)
					 {
						 last = DateTime.Now;
						 shouldStop.Reset();
						 while (!shouldStop.WaitOne(0) && DateTime.Now.Subtract(last).TotalMilliseconds < 250 && text != String.Empty)
							 Thread.Sleep(10);
						 if (shouldStop.WaitOne(0))
							 return;
					 }
					 PagedCollectionView pcv;
					 if (text == string.Empty)
						 pcv = new PagedCollectionView(FinanceSummaries);
					 else
						 pcv = new PagedCollectionView(FinanceSummaries.Where(f => f.Address.ToLower().Contains(text) || f.Name.ToLower().Contains(text)).ToList());
					 Dispatcher.BeginInvoke(new Action(() =>
						{
							financeSummaryDataGrid.ItemsSource = pcv;
							view.SortDescriptions.ToList().ForEach(z => pcv.SortDescriptions.Add(z));
						}));
				 })));
			t.Start();
		}

		DateTime lastClick = DateTime.MinValue;
		private void financeSummaryDataGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (DateTime.Now.Subtract(lastClick).TotalMilliseconds < 200)
			{
				var item = financeSummaryDataGrid.SelectedItem as FinanceSummary;
				if (item != null)
					Data.CurrentSummary = Data.Summaries.Where(s => s.PIN == item.PIN).First();
			}
			lastClick = DateTime.Now;
		}

	}
}
