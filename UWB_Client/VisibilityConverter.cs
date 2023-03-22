using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UWB_Client
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
            : this(true)
        {

        }
        public BoolToVisibilityConverter(bool collapsewhenInvisible)
            : base()
        {
            CollapseWhenInvisible = collapsewhenInvisible;
        }
        public bool CollapseWhenInvisible { get; set; }

        public Visibility FalseVisible
        {
            get
            {
                if (CollapseWhenInvisible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return (bool)value ? Visibility.Visible : FalseVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return ((Visibility)value == Visibility.Visible);
        }
    }
}
