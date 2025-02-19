using Dock.Enums;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Dock.Converters;

public sealed partial class SideToOrientationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Side side)
        {
            return side switch
            {
                Side.Left => Orientation.Vertical,
                Side.Right => Orientation.Vertical,
                Side.Top => Orientation.Horizontal,
                Side.Bottom => Orientation.Horizontal,
                _ => Orientation.Vertical
            };
        }

        return Orientation.Vertical;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
