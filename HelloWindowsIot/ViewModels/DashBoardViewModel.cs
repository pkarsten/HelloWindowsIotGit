using AppSettings;
using MSGraph;
using MSGraph.Response;
using RWPBGTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace HelloWindowsIot
{
    /// <summary>
    /// Represents a saved location for use in tracking travel time, distance, and routes. 
    /// </summary>
    public class DashBoardViewModel : BindableBase
    {

        #region Fields
        private string _taskResult;
        private string _taskProgress;
        private bool _enableClock;
        private DispatcherTimer _timer = new DispatcherTimer();
        #endregion

        #region Properties
        public DateTime CurrentTime { get { return DateTime.Now; } }
        public bool EnableClock
        {
            get { return this._enableClock; }
            set { this.SetProperty(ref this._enableClock, value); }
        }
        public BGTaskModel MyBgTask { get; set; }
        public RelayCommand ClickCmd { get; private set; }
        #endregion

        public DashBoardViewModel()
        {
            ClickCmd = new RelayCommand(DoSomething, () => true);

            // Update the Image times every 1 minutes.
            Helpers.StartTimer(0, 30, async () => await this.UpdateDashBoardImageAsync());

            GetCalendarEvents();
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachGetGraphData_ProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressGetGraphData);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedGetGraphData);
        }

        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private async void OnProgressGetGraphData(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    var progress = "Progress: " + args.Progress + "%";
                    _taskProgress = progress;
                    UpdateUI();
                }
                , CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedGetGraphData(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (Settings.LoadPictureListManually == true)
            {
                //Unregister App Trigger 
                BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.LoadImagesFromOneDriveTaskName);
                //Register Backgroundtask 
                var apptask = await BackgroundTaskConfig.RegisterBackgroundTask(MyBgTask.EntryPoint,
                                                                           Settings.LoadImagesFromOneDriveTaskName,
                                                                            await Dal.GetTimeIntervalForTask(Settings.LoadImagesFromOneDriveTaskName),
                                                                           null);
            }
            _taskProgress = "List Loaded";
            _taskResult = "";
            System.Diagnostics.Debug.WriteLine("OnCompleted Picturesloaded");
            Settings.LoadPictureListManually = false;
            UpdateUI();

        }

        private async void UpdateUI()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    System.Diagnostics.Debug.WriteLine("UpdateUI()");
                    //OnPropertyChanged("TaskResult");
                    //OnPropertyChanged("TaskProgress");
                }
                , CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Updates the Dashboard Image 
        /// </summary>
        private async Task UpdateDashBoardImageAsync()
        {
            await TaskFunctions.ChangeDashBoardBackGroundAsync(false);
            System.Diagnostics.Debug.WriteLine("Here we go");
            DashImage = Settings.DashBoardImage;
        }

        private async Task GetCalendarEvents()
        {
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);

            IList<CalendarEventItem> myevents = await graphService.GetCalendarEvents();
            calendarEvents = myevents.ToObservableCollection();


            IList<CalendarEventItem> myeventstoday = await graphService.GetTodayCalendarEvents();
            todayEvents = myeventstoday.ToObservableCollection();

            this.OnPropertyChanged("TodayCalendarEvents");
            this.OnPropertyChanged("NextCalendarEvents");
        }


        private void Timer_Tick(object sender, object e)
        {
                this.OnPropertyChanged("CurrentTime");
        }

        

        /// <summary>
        /// Load Settings /Setup Data for the ViewModel
        /// </summary>
        public async Task LoadData()
        {
            try
            {
                var s = await Dal.GetSetup();
                if (s.EnableClock == true)
                {
                    EnableClock = true;
                    _timer.Tick += Timer_Tick;
                    _timer.Interval = new TimeSpan(0, 0, 1);
                    _timer.Start();
                } else
                {
                    EnableClock = false;
                    _timer.Stop();
                }
                //this.OnPropertyChanged("EnableClock");

                var ts = Settings.ListBgTasks.Where(g => g.Name == Settings.LoadGraphDataTaskName).FirstOrDefault();
                if (ts != null)
                {
                    MyBgTask = ts;
                }
            }
            catch { }
        }

        private string name;
        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        private BitmapImage dashimage;

        /// <summary>
        /// Gets or sets the name of the Dashboard Image.
        /// </summary>
        public BitmapImage DashImage
        {
            get { return this.dashimage; }
            set { this.SetProperty(ref this.dashimage, value); }
        }

        private ObservableCollection<CalendarEventItem> calendarEvents = new ObservableCollection<CalendarEventItem>();
        /// <summary>
        /// Gets or sets the saved locations. 
        /// </summary>
        public ObservableCollection<CalendarEventItem> NextCalendarEvents
        {
            get { return this.calendarEvents; }
            set { this.SetProperty(ref this.calendarEvents, value); }
        }

        private ObservableCollection<CalendarEventItem> todayEvents = new ObservableCollection<CalendarEventItem>();
        /// <summary>
        /// Gets or sets the saved locations. 
        /// </summary>
        public ObservableCollection<CalendarEventItem> TodayCalendarEvents
        {
            get { return this.todayEvents; }
            set { this.SetProperty(ref this.todayEvents, value); }
        }

        /***************************************************/


        private string address;
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public string Address
        {
            get { return this.address; }
            set { this.SetProperty(ref this.address, value); }
        }

        private BasicGeoposition position;
        /// <summary>
        /// Gets the geographic position of the location.
        /// </summary>
        public BasicGeoposition Position
        {
            get { return this.position; }
            set
            {
                this.SetProperty(ref this.position, value);
                this.OnPropertyChanged(nameof(Geopoint));
            }
        }

        /// <summary>
        /// Gets a Geopoint representation of the current location for use with the map service APIs.
        /// </summary>
        public Geopoint Geopoint => new Geopoint(this.Position);

        private bool isCurrentLocation;
        /// <summary>
        /// Gets or sets a value that indicates whether the location represents the user's current location.
        /// </summary>
        public bool IsCurrentLocation
        {
            get { return this.isCurrentLocation; }
            set
            {
                this.SetProperty(ref this.isCurrentLocation, value);
                this.OnPropertyChanged(nameof(NormalizedAnchorPoint));
            }
        }

        private bool isSelected;
        /// <summary>
        /// Gets or sets a value that indicates whether the location is 
        /// the currently selected one in the list of saved locations.
        /// </summary>
        [IgnoreDataMember]
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                this.SetProperty(ref this.isSelected, value);
                this.OnPropertyChanged(nameof(ImageSource));
            }
        }

        /// <summary>
        /// Gets a path to an image to use as a map pin, reflecting the IsSelected property value. 
        /// </summary>
        public string ImageSource => IsSelected ? "Assets/mappin-yellow.png" : "Assets/mappin.png";

        private Point centerpoint = new Point(0.5, 0.5);
        private Point pinpoint = new Point(0.5, 0.9778);
        /// <summary>
        /// Gets a value for the MapControl.NormalizedAnchorPoint attached property
        /// to reflect the different map icon used for the user's current location. 
        /// </summary>
        public Point NormalizedAnchorPoint => IsCurrentLocation ? centerpoint : pinpoint;

        private MapRoute fastestRoute;
        /// <summary>
        /// Gets or sets the route with the shortest travel time to the 
        /// location from the user's current position.
        /// </summary>
        [IgnoreDataMember]
        public MapRoute FastestRoute
        {
            get { return this.fastestRoute; }
            set { this.SetProperty(ref this.fastestRoute, value); }
        }

        private int currentTravelTimeWithoutTraffic;
        /// <summary>
        /// Gets or sets the number of minutes it takes to drive to the location,
        /// without taking traffic into consideration.
        /// </summary>
        public int CurrentTravelTimeWithoutTraffic
        {
            get { return this.currentTravelTimeWithoutTraffic; }
            set { this.SetProperty(ref this.currentTravelTimeWithoutTraffic, value); }
        }

        private int currentTravelTime;
        /// <summary>
        /// Gets or sets the number of minutes it takes to drive to the location,
        /// taking traffic into consideration.
        /// </summary>
        public int CurrentTravelTime
        {
            get { return this.currentTravelTime; }
            set
            {
                this.SetProperty(ref this.currentTravelTime, value);
                this.OnPropertyChanged(nameof(FormattedCurrentTravelTime));
            }
        }

        /// <summary>
        /// Gets a display-string representation of the current travel time. 
        /// </summary>
        public string FormattedCurrentTravelTime =>
            this.CurrentTravelTime == 0 ? "??:??" :
            new TimeSpan(0, this.CurrentTravelTime, 0).ToString("hh\\:mm");

        private double currentTravelDistance;
        /// <summary>
        /// Gets or sets the current driving distance to the location, in miles.
        /// </summary>
        public double CurrentTravelDistance
        {
            get { return this.currentTravelDistance; }
            set
            {
                this.SetProperty(ref this.currentTravelDistance, value);
                this.OnPropertyChanged(nameof(FormattedCurrentTravelDistance));
            }
        }

        /// <summary>
        /// Gets a display-string representation of the current travel distance.
        /// </summary>
        public string FormattedCurrentTravelDistance =>
            this.CurrentTravelDistance == 0 ? "?? miles" :
            this.CurrentTravelDistance + " miles";

        private DateTimeOffset timestamp;
        /// <summary>
        /// Gets or sets a value that indicates when the travel info was last updated. 
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get { return this.timestamp; }
            set
            {
                this.SetProperty(ref this.timestamp, value);
                this.OnPropertyChanged(nameof(FormattedTimeStamp));
            }
        }

        /// <summary>
        /// Raises a change notification for the timestamp in order to update databound UI.   
        /// </summary>
        public void RefreshFormattedTimestamp() => this.OnPropertyChanged(nameof(FormattedTimeStamp));

        /// <summary>
        /// Gets a display-string representation of the freshness timestamp. 
        /// </summary>
        public string FormattedTimeStamp
        {
            get
            {
                double minutesAgo = this.Timestamp == DateTimeOffset.MinValue ? 0 :
                    Math.Floor((DateTimeOffset.Now - this.Timestamp).TotalMinutes);
                return $"{minutesAgo} minute{(minutesAgo == 1 ? "" : "s")} ago";
            }
        }

        private bool isMonitored;
        /// <summary>
        /// Gets or sets a value that indicates whether this location is 
        /// being monitored for an increase in travel time due to traffic. 
        /// </summary>
        public bool IsMonitored
        {
            get { return this.isMonitored; }
            set { this.SetProperty(ref this.isMonitored, value); }
        }

        /// <summary>
        /// Resets the travel time and distance values to 0, which indicates an unknown value.
        /// </summary>
        public void ClearTravelInfo()
        {
            this.CurrentTravelDistance = 0;
            this.currentTravelTime = 0;
            this.Timestamp = DateTimeOffset.Now;
        }

        private async void DoSomething()
        {

        }



    }
}
