using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace HelloWindowsIot
{
    public class SampleDashBoardData
    {
        /// <summary>
        /// Gets a list of four sample locations randomply positioned around the user's current 
        /// location or around the Microsoft main campus if the Geolocator is unavailable. 
        /// </summary>
        /// <returns>The sample locations.</returns>
        public static async Task<DashboardData> GetSampleDashBoardDataAsync()
        {
            var loccalendarEvents = new System.Collections.ObjectModel.ObservableCollection<MSGraph.Response.CalendarEventItem>();
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(-2) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 1", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 3", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(2) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 4", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(3) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 6", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(4) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 8", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(4) } });

            BitmapImage demoImage = new BitmapImage(new Uri("ms-appx:///[project-name]/Assets/dashdemoimage.jpg"));

            var dashboarddata = new DashboardData
            {
                Name = "MyDashBoard",
                DashImage = demoImage,
                CalendarEvents = loccalendarEvents
            };

            return dashboarddata;
        }
    }
}
