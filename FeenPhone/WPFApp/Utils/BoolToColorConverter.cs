using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FeenPhone.WPFApp
{
        public class BoolToColorConverter : IValueConverter
        {
            public Brush TrueColor { get; set; }
            public Brush FalseColor { get; set; }
            public Brush NullColor { get; set; }

            public BoolToColorConverter() { }

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                bool? bValue = (bool?)value;

                if (bValue.HasValue)
                    return bValue.Value ? TrueColor: FalseColor;

                return NullColor;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException("Conversion from color to object is not supported");
            }
        }
}
