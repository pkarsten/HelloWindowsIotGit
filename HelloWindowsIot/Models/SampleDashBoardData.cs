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
        public static async Task<DashBoardViewModel> GetSampleDashBoardDataAsync()
        {
            var loccalendarEvents = new System.Collections.ObjectModel.ObservableCollection<MSGraph.Response.CalendarEventItem>();
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(-2) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 1", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 3", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(2) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 4", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(3) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 6", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(4) } });
            loccalendarEvents.Add(new CalendarEventItem { Subject = "hallo Termin 8", StartDateTime = new DateInfoResponse { dateTime = DateTime.Now.AddDays(4) } });

            BitmapImage demoImage = new BitmapImage(new Uri("ms-appx:///Assets/dashdemoimage.jpg"));

            var dashboarddata = new DashBoardViewModel
            {
                Name = "MyDashBoard",
                DashImage = demoImage,
                NextCalendarEvents = loccalendarEvents,
                TodayCalendarEvents =loccalendarEvents,
                NextButtonText = "Next Btn Text err"
            };

            return dashboarddata;
        }

        public static async Task<SettingsViewModel> GetSampleSettingsDataAsync()
        {
            var viewmodel = new SettingsViewModel
            {
                SetupSettings = new UwpSqliteDal.Setup
                {
                    EnableLogging=true,
                    EnableClock = true,
                    EnablePictureAddOn = true,
                    EnableCalendarAddon = true,
                    EnableCalendarNextEvents = true,
                    EnableTodayEvents = true,
                    EnablePurchaseTask = true,
                    IntervalForLoadPictures = 15,
                    OneDrivePictureFolder = "/Bilder/WindowsIotApp",
                    TaskFolder = "AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoALgAAA9AbFx3CcYdHmhKEe93jcbkBAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAAA=/tasks",
                    PurchaseTaskID = "('AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoARgAAA9AbFx3CcYdHmhKEe93jcbkHAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAABM5OBFODyySJ-GWZ2VVJ2IAAGzZf5vAAAA')"
                }
            };
            return viewmodel;
        }
    }
}
