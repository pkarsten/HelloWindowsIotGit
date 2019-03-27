using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace HelloWindowsIot
{
    public static class Helpers
    {


        /// <summary>
        /// Runs the specified handler on the UI thread at Normal priority. 
        /// </summary>
        public static async Task CallOnUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler handler) =>
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);

        public static async Task CallOnMainViewUiThreadAsync(DispatchedHandler handler) =>
            await CallOnUiThreadAsync(CoreApplication.MainView.CoreWindow.Dispatcher, handler);

        /// <summary>
        /// Starts a timer to perform the specified action at the specified interval.
        /// </summary>
        /// <param name="intervalInMinutes">The interval.</param>
        /// <param name="action">The action.</param>
        public static void StartTimer(int intervalInMinutes, int intervalInSeconds, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, intervalInMinutes, intervalInSeconds);
            timer.Tick += (s, e) => action();
            timer.Start();
        }
    }
}
