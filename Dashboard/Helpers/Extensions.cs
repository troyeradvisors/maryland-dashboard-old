using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Dashboard
{
	public static class Extensions
	{
		//public static IEnumerable<T> Descendents<T>(this Panel panel) where T : UIElement
		//{
		//    return panel.Children.OfType<Panel>().Aggregate(panel.Children.OfType<T>(), (current, p) => current.Union(Descendents<T>(p)));
		//}

		public static IEnumerable<T> Descendants<T>(this UIElement x) where T : UIElement
		{
			//var children = Enumerable.Range(0,VisualTreeHelper.GetChildrenCount(x)).Select(i=>VisualTreeHelper.GetChild(x,i)).OfType<UIElement>();

			List<UIElement> children = new List<UIElement>();
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(x); ++i)
			{
				UIElement u = VisualTreeHelper.GetChild(x, i) as UIElement;
				if (u != null)
					children.Add(u);
			}
			return children.Aggregate(children.OfType<T>(), (acc, next) => acc.Union(Descendants<T>(next)));
		}
	}
}
