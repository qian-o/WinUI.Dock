using Dock.Enums;
using Microsoft.UI.Xaml.Data;

namespace Dock.Converters;

public sealed partial class SideToAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (Side)value switch
        {
            Side.Left or Side.Right => 90,
            _ => 0
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
