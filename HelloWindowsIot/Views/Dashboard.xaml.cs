using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using RWPBGTasks;
using Windows.ApplicationModel.Background;
using AppSettings;
using MSGraph;

namespace HelloWindowsIot
{
    public sealed partial class DashBoard : Page
    {
        /// <summary>
        /// Gets or sets the DashBoardData . 
        /// </summary>
        public DashBoardViewModel ViewModel { get; set; }

        public DashBoard()
        {
            this.InitializeComponent();
            this.ViewModel = new DashBoardViewModel();

            // Update the Image times every 1 minutes.
            Helpers.StartTimer(0,30, async () => await this.UpdateDashBoardImageAsync());

            // MappedLocations is a superset of Locations, so any changes in Locations
            // need to be reflected in MappedLocations. 
            //this.ViewModel.TodayCalendarEvents.CollectionChanged += (s, e) =>
            //{
            //    if (e.NewItems != null) foreach (CalendarEventItem item in e.NewItems) this.ViewModel.TodayCalendarEvents.Add(item);
            //    if (e.OldItems != null) foreach (CalendarEventItem item in e.OldItems) this.ViewModel.TodayCalendarEvents.Remove(item);
            //};
            //this.ViewModel.NextCalendarEvents.CollectionChanged += (s, e) =>
            //{
            //    if (e.NewItems != null) foreach (CalendarEventItem item in e.NewItems) this.ViewModel.NextCalendarEvents.Add(item);
            //    if (e.OldItems != null) foreach (CalendarEventItem item in e.OldItems) this.ViewModel.NextCalendarEvents.Remove(item);
            //};
        }

        

        /// <summary>
        /// Updates the Dashboard Image 
        /// </summary>
        private async Task UpdateDashBoardImageAsync()
        {
            await TaskFunctions.ChangeDashBoardBackGroundAsync(false);//. LoadImageForDesktop(ItemInfoResponse item);
            System.Diagnostics.Debug.WriteLine("Here we go");
            ViewModel.DashImage = Settings.DashBoardImage;
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachChangeImageProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            //task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            //task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }

        /// <summary>
        /// Loads the saved Dashboard data on first navigation, and 
        /// attaches a Geolocator.StatusChanged event handler. 
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                
                // Load location data from storage if it exists;
                // otherwise, load sample location data.
                
                var dashboarddata = await SampleDashBoardData.GetSampleDashBoardDataAsync();
                ViewModel = dashboarddata;
                //await ViewModel.LoadData();

                //var locations = await LocationDataStore.GetLocationDataAsync();
                //if (locations.Count == 0) locations = await LocationDataStore.GetSampleLocationDataAsync();
                //foreach (var location in locations) this.Locations.Add(location);

                // Start handling Geolocator and network status changes after loading the data 
                // so that the view doesn't get refreshed before there is something to show.
                //LocationHelper.Geolocator.StatusChanged += Geolocator_StatusChanged;
                //NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            }
        }
    }
}
