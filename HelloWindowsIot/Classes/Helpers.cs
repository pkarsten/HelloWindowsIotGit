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
