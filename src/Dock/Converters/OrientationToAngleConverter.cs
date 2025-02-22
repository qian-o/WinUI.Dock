using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Dock.Converters;

public partial class OrientationToAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Orientation orientation)
        {
            return orientation switch
            {
                Orientation.Horizontal => 0,
                Orientation.Vertical => 90,
                _ => 0
            };
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
