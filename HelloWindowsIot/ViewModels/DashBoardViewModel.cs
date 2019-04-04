using AppSettings;
using MSGraph;
using MSGraph.Response;
using RWPBGTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using HelloWindowsIot;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using UwpSqLiteDal;

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
        private BitmapImage dashimage;
        private DispatcherTimer _dtimer = new DispatcherTimer(); //For Clock 
        private ObservableCollection<CalendarEvent> todayEvents = new ObservableCollection<CalendarEvent>();
        private ObservableCollection<CalendarEvent> nextcalendarEvents = new ObservableCollection<CalendarEvent>();
        private PurchTask purchtask =new PurchTask();
        private string purchtaskcontent = "";
        private string purchtasksubject ="";
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
        public BitmapImage DashImage
        {
            get { return this.dashimage; }
            set { this.SetProperty(ref this.dashimage, value); }
        }
        public ObservableCollection<CalendarEvent> NextCalendarEvents
        {
            get { return this.nextcalendarEvents; }
            set { this.SetProperty(ref this.nextcalendarEvents, value); }
        }
        public ObservableCollection<CalendarEvent> TodayCalendarEvents
        {
            get { return this.todayEvents; }
            set { this.SetProperty(ref this.todayEvents, value); }
        }
        public PurchTask PurchTask
        {
            get { return this.purchtask; }
            set
            {
                this.SetProperty(ref this.purchtask, value);
            }
        }
        public string PurchTaskContent
        {
            get { return this.purchtaskcontent; }
            set { this.SetProperty(ref this.purchtaskcontent, value); }
        }
        public string PurchTaskSubject
        {
            get { return this.purchtasksubject; }
            set { this.SetProperty(ref this.purchtasksubject, value); }
        }
        #endregion

        #region constructor
        public DashBoardViewModel()
        {
            ClickCmd = new RelayCommand(DoSomething, () => true);
        }
        #endregion

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
            try
            {
                _taskProgress = "Graph Data ";
                _taskResult = "";

                var t = Dal.GetNextEvents().ToObservableCollection();

                nextcalendarEvents = t;

                var t1 = Dal.GetTodayEvents().ToObservableCollection();
                todayEvents = t1;
                var pt = await Dal.GetPurchTask();
                purchtaskcontent = pt.BodyText.Replace("<li> </li>", ""); 
                purchtasksubject = pt.Subject;
                await Dal.SaveLogEntry(LogType.Info, "Purch task" + purchtasksubject);
                UpdateUI();
            }
            catch(Exception ex)
            {
                await Dal.SaveLogEntry(LogType.Error, "Exce in OnCompletedGraphtask " + ex.Message);
            }
            finally
            {
                await Dal.SaveLogEntry(LogType.Info, "OnCompleted Load Graph Data");
            }
        }

        private async void UpdateUI()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    System.Diagnostics.Debug.WriteLine("UpdateUI()");
                    this.OnPropertyChanged("TodayCalendarEvents");
                    this.OnPropertyChanged("NextCalendarEvents");
                    this.OnPropertyChanged("PurchTaskContent");
                    this.OnPropertyChanged("PurchTaskSubject");
                    //OnPropertyChanged("TaskProgress");
                }
                , CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Updates the Dashboard Image 
        /// </summary>
        private async Task UpdateDashBoardImageAsync()
        {
            System.Diagnostics.Debug.WriteLine("Here we go ->  Called at " + DateTime.Now);
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                async () =>
                    {
                        DashImage = await HelperFunc.StreamImageFromOneDrive();
                    }
                    , CoreDispatcherPriority.High);

            UpdateUI();
        }
        /// <summary>
        /// Load Initial Settings /Setup Data for the ViewModel
        /// </summary>
        public async Task LoadData()
        {
            try
            {

                var ts = Settings.ListBgTasks.Where(g => g.Name == Settings.LoadGraphDataTaskName).FirstOrDefault();
                if (ts != null)
                {
                    MyBgTask = ts;
                }

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == Settings.LoadGraphDataTaskName)
                    {
                        System.Diagnostics.Debug.WriteLine("Task " + task.Value.Name + " is registriert attach handler ");
                        AttachGetGraphData_ProgressAndCompletedHandlers(task.Value);
                    }
                }

                var s = await Dal.GetSetup();
                await CheckClockStatus(s);

                if (s.IntervalForDiashow < 60) { s.IntervalForDiashow = 60; };

                Helpers.StartTimer(0, s.IntervalForDiashow, async () => await this.UpdateDashBoardImageAsync());
            }
            catch(Exception ex)
            {
                await Dal.SaveLogEntry(LogType.Error, ex.Message);
            }
            finally
            {
                UpdateUI();
            }
        }

        #region Clock
        private async Task CheckClockStatus(Setup setup)
        {
            if (setup.EnableClock == true)
            {
                await StartClock();
            }
            else
            {
                await StopClock();
            }
        }

        private async Task StartClock()
        {
            EnableClock = true;
            _dtimer.Tick += Timer_Tick;
            _dtimer.Interval = new TimeSpan(0, 0, 1);
            _dtimer.Start();
        }
        private async Task StopClock()
        {
            EnableClock = false;
            _dtimer.Stop();
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Change Time on CLock if Clock COntrol is Enable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            this.OnPropertyChanged("CurrentTime");
        }

        private async void DoSomething()
        {

        }
        #endregion
    }
}
