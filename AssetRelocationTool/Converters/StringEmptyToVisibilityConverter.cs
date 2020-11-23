using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AssetRelocationTool.Converter
{
	[ValueConversion(typeof(string), typeof(Visibility))]
	class StringEmptyToVisibilityConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
