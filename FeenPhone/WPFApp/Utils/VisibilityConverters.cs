using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FeenPhone.WPFApp
{
    public class BoolToVisiblityConverter : IValueConverter
    {
        public bool Collapse { get; set; }
        public bool Inverse { get; set; }

        public BoolToVisiblityConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;

            if (bValue != Inverse)
            {
                return Visibility.Visible;
            }
            else
            {
                if (Collapse)
                    return Visibility.Collapsed;
                else
                    return Visibility.Hidden;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
                return !Inverse;
            else
                return Inverse;

        }
    }

    public class NullToVisiblityConverter : IValueConverter
    {
        public bool Collapse { get; set; }
        public bool Inverse { get; set; }

        public NullToVisiblityConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = value != null;
            if (bValue != Inverse)
            {
                return Visibility.Visible;
            }
            else
            {
                if (Collapse)
                    return Visibility.Collapsed;
                else
                    return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Conversion from visibility to object is not supported");
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public InverseBoolConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool?)value != true;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool?)value == true;

        }
    }

}
