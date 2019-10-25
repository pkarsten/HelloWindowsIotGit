using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HelloWindowsIot
{
    public static class Extensions
    {
        public static bool AllEqual(this bool firstValue, params bool[] bools)
        {
            return bools.All(thisBool => thisBool == firstValue);
        }

        #region WebView Extension
        // TODO: For what is this? WebView = some thing like IFRAMe? Need it? 
        //h ttps://code.msdn.microsoft.com/windowsapps/How-to-bind-HTML-from-a-7a6ff47c
        public static string GetHTML(DependencyObject obj)
        {
            return (string)obj.GetValue(HTMLProperty);
        }

        public static void SetHTML(DependencyObject obj, string value)
        {
            obj.SetValue(HTMLProperty, value);
        }

        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc...  
        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached("HTML", typeof(string), typeof(Extensions), new PropertyMetadata(0, new PropertyChangedCallback(OnHTMLChanged)));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            WebView wv = d as WebView;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
        #endregion
    }
}
