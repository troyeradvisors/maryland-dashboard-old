namespace Dashboard
{
    using System;
    using System.Windows.Data;



    public class FormatConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (parameter != null)
			{
				string formatterString = parameter.ToString();

				if (!string.IsNullOrEmpty(formatterString))
				{
					return string.Format(culture, formatterString, value);
				}
			}

			return value.ToString();
		}

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}