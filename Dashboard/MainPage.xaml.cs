namespace Dashboard
{
	using System.Windows.Threading;
	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Navigation;
	using Dashboard.LoginUI;
	using System.ServiceModel.DomainServices.Client;
	using Dashboard.Web.Services;
	using Dashboard.Web;
	using System.Linq;
	using System.Threading;
	using System.Windows.Printing;
	using System.Windows.Media.Imaging;
	using System.Windows.Media;
	using System.Windows.Shapes;
	using System.Windows.Data;
	using System.Net;
	/// <summary>
	/// <see cref="UserControl"/> class providing the main UI for the application.
	/// </summary>
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();
			//this.LoginContainer.Child = new LoginStatus();


			tbCopyright.Text = "©" + DateTime.Now.Year + " Troyer Advisors, LLC. All rights reserved.\r\nUser accepts full responsibility for reliance on data.";

            Data.HomeChanged += NursingHome_Changed;
            Data.FiscalYearChanged += (x, y) => LoadSummaries();

            Data.LoadFiscalYears((x, y) =>
                {
                    cbFiscalYear.ItemsSource = Data.FiscalYears;
                    cbFiscalYear.SelectedIndex = 0;
                });

			Data.LoadCurrentClient((x, y) =>
				{
					if (Data.CurrentClient == null || Data.CurrentClient.Logo == null)
						return;
					var b = new BitmapImage(new Uri(Data.CurrentClient.Logo));
					b.ImageFailed += (xx, yy) =>
					{ 
						b.UriSource = new Uri("Images/troyeradvisors.png", UriKind.Relative); // Need to set it to some valid image or pages only print white pages... why? Only God knows.
					};
					b.ImageOpened += (xx, yy) => imgLogo.Visibility = System.Windows.Visibility.Visible;
					imgLogo.Source = b;

				});
		}

        private void LoadSummaries()
        {
            Data.LoadSummaries((xx, yy) =>
            {
                var summaries = Data.SummariesThisYear.ToList();
                cbSummaries.ItemsSource= summaries;
                if (lastHome != null) // Fiscal year changed, which means try to find home with same PIN as last fiscal year.
                {
                    var newHome = summaries.Where(e => e.PIN == lastHome.PIN).FirstOrDefault();
                    if (newHome != null)
                        cbSummaries.SelectedItem = newHome;
                    else
                        cbSummaries.SelectedIndex = 0;
                }
                else cbSummaries.SelectedIndex = 0;
            });
        }

		/// <summary>
		/// After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
		/// </summary>
		private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
		{

			foreach (UIElement child in LinksStackPanel.Children)
			{
				HyperlinkButton hb = child as HyperlinkButton;
				if (hb != null && hb.NavigateUri != null)
				{
					if (hb.NavigateUri.ToString().Equals(e.Uri.ToString()))
					{
						VisualStateManager.GoToState(hb, "ActiveLink", true);
					}
					else
					{
						VisualStateManager.GoToState(hb, "InactiveLink", true);
					}
				}
			}

			Timer = new DispatcherTimer();
			Timer.Interval = TimeSpan.FromSeconds(.55);
			Timer.Tick += (x, y) =>
			{
				if (desiredUri != null)
				{
					ContentFrame.Navigating -= ContentFrame_Navigating;
					ContentFrame.Navigate(desiredUri);
					ContentFrame.Navigating += ContentFrame_Navigating;
					desiredUri = null;
				}
				Timer.Stop();
			};
			Timer.Start();

		}

		/// <summary>
		/// If an error occurs during navigation, show an error window
		/// </summary>
		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			e.Handled = true;
			ErrorWindow.CreateNew(e.Exception);
		}

		private void nursingHomeSummaryDDS_LoadedData(object sender, System.Windows.Controls.LoadedDataEventArgs e)
		{

			if (e.HasError)
			{
				System.Windows.MessageBox.Show(e.Error.ToString(), "Load Error", System.Windows.MessageBoxButton.OK);
				e.MarkErrorAsHandled();
			}
		}

        HomeSummary lastHome;
		private void NursingHome_Changed(object sender, EventArgs e)
		{
            lastHome = Data.CurrentSummary;
			if (cbSummaries.SelectedItem != Data.CurrentSummary)
				cbSummaries.SelectedItem = Data.CurrentSummary;
			Data.Load(Data.CurrentHome, (x, y) => stackNursingHome.DataContext = Data.CurrentHome);
			Data.Load(Data.CurrentDescription, (x, y) => tbFiscalDateRange.Text = Data.CurrentDescription.FiscalDateRange);
			Data.Load(Data.CurrentFinances, (x, y) => tbBeds.Text = Data.CurrentFinances.CertifiedBedCount.ToString());
		}

		private void btnPrint_Click(object sender, RoutedEventArgs e)
		{
			Printer printer = new Printer(this);
			printer.Print("Dashboard - " + (ContentFrame.Content as Page).Title);
		}


		DispatcherTimer Timer = new DispatcherTimer();
		Uri desiredUri = null;
		private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			if (Timer.IsEnabled)
			{
				e.Cancel = true;
				desiredUri = e.Uri;
			}
		}



		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
		}



	}


}