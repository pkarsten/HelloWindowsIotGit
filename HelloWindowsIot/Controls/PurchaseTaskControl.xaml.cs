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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace HelloWindowsIot.Controls
{
    public sealed partial class PurchaseTaskControl : UserControl
    {
        public PurchaseTaskControl()
        {
            this.InitializeComponent();
            GetPurchList();
        }

        private async Task GetPurchList()
        {
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);
            //CalendarText.Text = await graphService.GetCalendarViewTest();
            string s = "";
            var mypurchtask = await graphService.GetPurchaseTask();
            TaskWebView.NavigationCompleted += WebView_NavigationCompleted;
            TaskSubject.Text = mypurchtask.Subject;
            var content = mypurchtask.TaskBody.Content;
            content = content.Replace("<li> </li>", "");
            TaskWebView.NavigateToString(content);
            System.Diagnostics.Debug.WriteLine("My Purch Task: " + mypurchtask.Subject);
            System.Diagnostics.Debug.WriteLine("========================================");
            System.Diagnostics.Debug.WriteLine("My Purch List" + mypurchtask.TaskBody.Content);
            System.Diagnostics.Debug.WriteLine("========================================");
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
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
