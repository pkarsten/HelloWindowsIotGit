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

namespace HelloWindowsIot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
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
