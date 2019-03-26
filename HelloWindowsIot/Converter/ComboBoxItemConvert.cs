using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpSqLiteDal;
using Windows.UI.Xaml.Data;

namespace HelloWindowsIot
{
    public class ComboBoxTaskFolderItemConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as MSGraph.Response.TaskFolder;
        }
    }

    public class ComboBoxTaskResponseItemConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as MSGraph.Response.TaskResponse;
        }
    }
}
