using MSGraph;
using MSGraph.Response;
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
    public sealed partial class InfoBox : UserControl
    {
        public InfoBox()
        {
            this.InitializeComponent();
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

            CalenderEventText();
        }

        public async void CalenderEventText()
        {
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);
            string s = "";

            IList<CalendarEventItem> myevents = await graphService.GetCalendarEvents(20);
            foreach (CalendarEventItem ce in myevents)
            {
                s = s + "Date : " + ce.StartDateTime.dateTime + " Subject: " + ce.Subject + " \n";
                System.Diagnostics.Debug.WriteLine("Date : " + ce.StartDateTime.dateTime + " Subject: " + ce.Subject);
            }

            MyEvents.Text = s;
        }

        DispatcherTimer Timer = new DispatcherTimer();

        private void Timer_Tick(object sender, object e)
        {
            MyTime.Text = DateTime.Now.ToString("h:mm:ss tt");
        }
    }
}
