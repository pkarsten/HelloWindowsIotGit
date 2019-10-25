using MSGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace HelloWindowsIot.Controls
{
    public sealed partial class PurchaseTaskControl : UserControl
    {
        public static readonly DependencyProperty PurchTaskHtmlProperty =
          DependencyProperty.Register("PurchTaskHtml", typeof(string), typeof(PurchaseTaskControl),null);

        public string PurchTaskHtml
        {
            get => (string)GetValue(PurchTaskHtmlProperty);
            set => SetValue(PurchTaskHtmlProperty, value);
        }
        public static readonly DependencyProperty PurchTaskSubjectProperty =
  DependencyProperty.Register("PurchTaskSubjectProperty ", typeof(string), typeof(PurchaseTaskControl), new PropertyMetadata(0, new PropertyChangedCallback(OnPurchTaskSubjectChanged)));

        public string PurchTaskSubject
        {
            get => (string)GetValue(PurchTaskSubjectProperty);
            set => SetValue(PurchTaskSubjectProperty, value);
        }

        public PurchaseTaskControl()
        {
            this.InitializeComponent();
            TaskWebView.NavigationCompleted += WebView_NavigationCompleted;
            // TODO: See extension function, remove it? 
            //TaskWebView.NavigateToString(PurchTaskHtml);
            //GetPurchList();
        }

        private static void OnPurchTaskSubjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

           
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            ///Calc Needed High for Webview
            var webView = sender as WebView;
            webView.Height = 30;
            int width;
            int height;

            // get the total width and height
            var widthString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
            var heightString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });

            if (!int.TryParse(widthString, out width))
            {
                throw new Exception("Unable to get page width");
            }
            if (!int.TryParse(heightString, out height))
            {
                throw new Exception("Unable to get page height");
            }

            // resize the webview to the content
            webView.Width = width;
            webView.Height = height;
        }
    }
}
