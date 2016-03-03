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
using Dashboard.Controls;

namespace Dashboard
{
	public interface ICustomPrint
	{
		void PrePrint(Size printSize);
		void PostPrint();
		bool WantPrint { get; }
	}

	public class Printer
	{
		//const double MIN_SCALE = .679;
		//const double MIN_SCALE = .6243;
		public const double MIN_SCALE = .5;

		MainPage Content;
		Grid PrintContainer = new Grid();
		LayoutTransformer PrintTransform = new LayoutTransformer();
		Grid PrintFrame = new Grid();
		Dashboard.Controls.BusyIndicator OriginalContainer;
		PrintDocument PrintDocument = new PrintDocument();
		Grid Footer;



		public Printer(MainPage content)
		{
			Content = content;
			Footer = CreateFooter();
			PrintDocument.PrintPage += p_PrintPage;
			PrintDocument.EndPrint += new EventHandler<EndPrintEventArgs>(p_EndPrint);
			PrintDocument.BeginPrint += new EventHandler<BeginPrintEventArgs>(p_BeginPrint);
		}

		public void Print(string name = "")
		{
			ICustomPrint custom = Content.ContentFrame.Content as ICustomPrint;
			if (custom == null || custom.WantPrint)
				PrintDocument.Print(name == "" ? Guid.NewGuid().ToString() : name);
		}

		void p_BeginPrint(object sender, BeginPrintEventArgs args)
		{
			OriginalContainer = Application.Current.RootVisual as Dashboard.Controls.BusyIndicator;
			OriginalContainer.Content = new Image() { Source = new WriteableBitmap(Content, null) };
		
			NewPrintJob = true;
		}



		Size ScaledSize;
		Point PrintPosition;
		bool NewPrintJob = true;

		void PrePrint(PrintPageEventArgs page)
		{
			if (!NewPrintJob) return;

			PrintContainer.Children.Add(Content);
			//ReplaceScrollViewers(Content, x=>(x as ScrollViewer).Name == "gridScrollViewer");
			FixForPrinting(Content, page);

			TiledBackground tb = Content.GridRoot.Children.FirstOrDefault(e => e.GetType() == typeof(TiledBackground)) as TiledBackground;
			Content.GridRoot.Children.Remove(tb);
			PrintContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			PrintContainer.Width = PrintContainer.DesiredSize.Width;
			PrintContainer.Height = PrintContainer.DesiredSize.Height;
			PrintContainer.UpdateLayout();
			Content.GridRoot.Children.Insert(0, tb);

			PrintTransform.Content = PrintContainer;
			PrintFrame.VerticalAlignment = VerticalAlignment.Top;
			PrintFrame.HorizontalAlignment = HorizontalAlignment.Left;
			PrintFrame.Children.Add(PrintTransform);
			PrintFrame.Children.Add(Footer);


			Point s = new Point(page.PrintableArea.Width / PrintContainer.Width, page.PrintableArea.Height / PrintContainer.Height);
			double scale;
			if (s.X > MIN_SCALE && s.Y > MIN_SCALE)
				scale = Math.Min(s.X, s.Y);
			else if (s.X > MIN_SCALE)
				scale = s.X;
			else if (s.Y > MIN_SCALE)
				scale = s.Y;
			else
				scale = MIN_SCALE;
			scale = Math.Min(1, scale);
			ScaledSize = new Size(scale * PrintContainer.Width, scale * PrintContainer.Height);
			PrintPosition = new Point(0, 0);
			PrintTransform.LayoutTransform = new ScaleTransform() { ScaleX = scale, ScaleY = scale };
			PrintTransform.UpdateLayout();
			NewPrintJob = false;
		}

		void p_PrintPage(object sender, PrintPageEventArgs p)
		{
			PrePrint(p);
			PrintTransform.Margin = new Thickness(-PrintPosition.X, -PrintPosition.Y, 0, 0);
			p.PageVisual = PrintFrame;
			p.PageVisual.UpdateLayout();

			PrintPosition.X += p.PrintableArea.Width;
			if (PrintPosition.X >= ScaledSize.Width-1e-6)
			{
				PrintPosition.X = 0;
				PrintPosition.Y += p.PrintableArea.Height;
			}
			p.HasMorePages = PrintPosition.Y < ScaledSize.Height-1e-6;
		}

		void p_EndPrint(object sender, EndPrintEventArgs e)
		{
			(Content.Parent as Grid).Children.Clear();
			UnfixAfterPrinting(Content);
			OriginalContainer.Content = Content;
			NewPrintJob = false;
		}

		void ReplaceScrollViewers(UIElement x, Func<UIElement,bool> predicate = null)
		{
			ScrollViewer viewer = x as ScrollViewer;
			if (viewer != null && (predicate == null || predicate(viewer)))
			{
				UIElement child = viewer.Content as UIElement;
				viewer.Width = viewer.ExtentWidth * 1.1;
				viewer.Height = viewer.ExtentHeight * 1.1;
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(x); ++i)
				ReplaceScrollViewers(VisualTreeHelper.GetChild(x, i) as UIElement);
		}


		void FixForPrinting(UIElement x, PrintPageEventArgs page)
		{
			//var c = x as Competition;
			//if (c != null)
			//{
			//    var scroll = c.FindName("gridScrollViewer") as ScrollViewer;
			//    scroll.Width = scroll.ExtentWidth * 1.1;
			//    scroll.Height = scroll.ExtentHeight * 1.1;
			//}

			//foreach (var s in x.Descendants<ScrollViewer>().OfType<ScrollViewer>().Where(e => e.Name.Equals("gridScrollViewer")))
			//{
			//    s.Width = s.ExtentWidth * 1.1;
			//    s.Height = s.ExtentHeight * 1.1;
			//    //s.Width = s.ExtentWidth * 1.1;
			//    //s.Height = s.ExtentHeight * 1.1;
			//}

			foreach (var c in x.Descendants<UIElement>().OfType<ICustomPrint>())
				c.PrePrint(page.PrintableArea);

			foreach (var data in x.Descendants<DataGrid>())
			{
				var p = data.ItemsSource as PagedCollectionView;
				data.Height = 28 * (p.Count + 2);
				data.Width = 1.1 * data.Columns.Aggregate(0.0, (acc, next) => acc += next.ActualWidth);  // need to +10% to offset virtualization
			}

			Content.tbCopyright.Visibility = Visibility.Collapsed;

			x.UpdateLayout();
		}
		void UnfixAfterPrinting(UIElement x)
		{
			foreach (var c in x.Descendants<UIElement>().OfType<ICustomPrint>())
				c.PostPrint();

			Content.tbCopyright.Visibility = Visibility.Visible;

			foreach (var data in x.Descendants<DataGrid>())
				data.Height = data.Width = double.NaN;
		}


		public Grid CreateFooter()
		{
			Grid grid = new Grid() { HorizontalAlignment = System.Windows.HorizontalAlignment.Right, VerticalAlignment = System.Windows.VerticalAlignment.Bottom, Margin = new Thickness(0,0,5,7) };
			TextBlock tb2 = new TextBlock();
			tb2.Text = "©" + DateTime.Now.Year + " Troyer Advisors, LLC. All rights reserved. \r\nUser accepts full responsibility for reliance on data";
			tb2.FontSize = 8;
			tb2.Foreground = new SolidColorBrush(Colors.Black);
			tb2.Opacity = .5;
			grid.Children.Add(tb2);
			return grid;
		}

	}
}
