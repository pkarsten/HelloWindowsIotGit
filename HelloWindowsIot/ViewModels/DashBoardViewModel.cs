﻿using AppSettings;
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
using HelloWindowsIot.Models;

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
        private string dashdescription;
        private DispatcherTimer _dtimer = new DispatcherTimer(); //For Clock 
        private ObservableCollection<CalendarEvent> todayEvents = new ObservableCollection<CalendarEvent>();
        private ObservableCollection<CalendarEvent> nextcalendarEvents = new ObservableCollection<CalendarEvent>();
        private ObservableCollection<ToDoTask> todotasks =new ObservableCollection<ToDoTask>();
        private InfoModel _infoM = new InfoModel();
        private string purchtaskcontent = "";
        private string purchtasksubject ="";
        private string tasklisttitel;
        private double _myoffset;
        #endregion

        #region Properties
        private DateTime _currentTime = DateTime.UtcNow;
        public DateTime CurrentTime { 
            
            get 
            {
                DateTime mydt = DateTime.UtcNow.AddHours(_myoffset);
                return mydt;
            }
        } 
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

        public string DashImageDescription
        {
            get { return this.dashdescription; }
            set { this.SetProperty(ref this.dashdescription, value); }
        }
        public string TaskListTitel
        {
            get { return this.tasklisttitel; }
            set { this.SetProperty(ref this.tasklisttitel, value); }
        }
        public bool HasDescription
        {
            get
            {
                return string.IsNullOrEmpty(this.dashdescription);
            }
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
        public ObservableCollection<ToDoTask> ToDoTasks
        {
            get { return this.todotasks; }
            set
            {
                this.SetProperty(ref this.todotasks, value);
            }
        }

        public InfoModel InfoM
        {
            get { return this._infoM; }
            set { this.SetProperty(ref this._infoM, value); }
        }
        public string ToDoTaskContent
        {
            get { return this.purchtaskcontent; }
            set { this.SetProperty(ref this.purchtaskcontent, value); }
        }
        public string ToDoTaskSubject
        {
            get { return this.purchtasksubject; }
            set { this.SetProperty(ref this.purchtasksubject, value); }
        }

        private bool hideTodayEvents;
        public bool HideTodayEvents
        {
            get { return this.hideTodayEvents; }
            set
            {
                this.SetProperty(ref this.hideTodayEvents, value);
                System.Diagnostics.Debug.WriteLine("Hide today " + value +  " ~ " + this.hideTodayEvents);
            }
        }

        private bool showTasks;
        public bool ShowTasks
        {
            get { return showTasks; }
            set
            {
                this.SetProperty(ref this.showTasks, value);
            }
        }

        private bool showTodayEvents;
        public bool ShowTodayEvents
        {
            get { return showTodayEvents; }
            set
            {
                this.SetProperty(ref this.showTodayEvents, value);
            }
        }
        private bool showNextEvents;
        public bool ShowNextEvents
        {
            get { return showNextEvents; }
            set
            {
                this.SetProperty(ref this.showNextEvents, value);
            }
        }
        private bool showCalendarAddOn;
        public bool ShowCalendarAddOn
        {
            get { return showCalendarAddOn; }
            set
            {
                this.SetProperty(ref this.showCalendarAddOn, value);
            }
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

                await LoadCalendarEvents();
                await LoadPurchTask();
                UpdateUI();
            }
            catch(Exception ex)
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception: in OnCompletedGetGraphData: " + ex.Message);
            }
            finally
            {
                
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "OnCompleted Load Graph Data");
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
                    this.OnPropertyChanged("ToDoTasks");
                    this.OnPropertyChanged("ToDoTaskContent");
                    this.OnPropertyChanged("TaskListTitel");
                    this.OnPropertyChanged("ToDoTaskSubject");
                    this.OnPropertyChanged("EnableCLock");
                    this.OnPropertyChanged("CurrentTime");
                    this.OnPropertyChanged("HideTodayEvents");
                    this.OnPropertyChanged("InfoM");
                    this.OnPropertyChanged("DashImageDescription");
                    this.OnPropertyChanged("HasDescription");
                    this.OnPropertyChanged("ShowTasks");
                }
                , CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Updates the Dashboard Image 
        /// </summary>
        private async Task UpdateDashBoardImageAsync()
        {
            try
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(
                async () =>
                    {
                        var getimage = await HelperFunc.StreamImageFromOneDrive();
                        if (getimage != null)
                        {
                            DashImage = getimage.Photo;
                            DashImageDescription = getimage.Description;
                        }
                    }
                    , CoreDispatcherPriority.High);
            }
            catch (Exception ex) { }
            finally
            {
                UpdateUI();
            }

            try
            {
                await LoadDatabaseInfos();
            }
            catch (Exception ex) { }
            finally
            {
                UpdateUI();
            }
            
        }
        /// <summary>
        /// Load Initial Settings /Setup Data for the ViewModel
        /// </summary>
        public async Task LoadData()
        {
            try
            {
                await this.UpdateDashBoardImageAsync();
                await LoadCalendarEvents();
                await LoadPurchTask();
                await LoadDatabaseInfos();

                var ts = BGTasksSettings.ListBgTasks.Where(g => g.Name == BGTasksSettings.LoadGraphDataTaskName).FirstOrDefault();
                if (ts != null)
                {
                    MyBgTask = ts;
                }

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == BGTasksSettings.LoadGraphDataTaskName)
                    {
                        System.Diagnostics.Debug.WriteLine("Task " + task.Value.Name + " is registriert attach handler ");
                        AttachGetGraphData_ProgressAndCompletedHandlers(task.Value);
                    }
                }

                var s = await DAL.AppDataBase.GetSetup();
                if(s != null)
                {
                    _myoffset = s.EventsOffset;
                }
                else
                {
                    _myoffset = Configuration.InitialSetupConfig.EventsOffset;
                }
                
                await CheckClockStatus(s);

                if (s.IntervalForDiashow < 60) { s.IntervalForDiashow = 60; };

                Helpers.StartTimer(0, s.IntervalForDiashow, async () => await this.UpdateDashBoardImageAsync());
                ShowTasks = !s.EnablePurchaseTask;
                ShowTodayEvents = !s.EnableTodayEvents;
                ShowNextEvents = !s.EnableCalendarNextEvents;
                ShowCalendarAddOn = !s.EnableCalendarAddon;
                UpdateUI();

            }
            catch(Exception ex)
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, ex.Message);
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

        /// <summary>
        /// Change Time on CLock if Clock COntrol is Enable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            this.OnPropertyChanged("CurrentTime");
        }
        #endregion

        #region Calendar Events
        private async Task LoadCalendarEvents()
        {
            var t = DAL.AppDataBase.GetNextEvents().ToObservableCollection();
            if (t != null)
            {
                nextcalendarEvents = t;
            }

            if (t == null)
            {
                nextcalendarEvents = null;
            }

            var t1 = DAL.AppDataBase.GetTodayEvents().ToObservableCollection();
            if (t1.Count > 0)
            {
                todayEvents = t1;
                hideTodayEvents = false;
                HideTodayEvents = false;
            }
            else
            {
                todayEvents = null;
                hideTodayEvents = true;
                HideTodayEvents = true;
            }
        }
        #endregion

        #region ToDoTasks
        private async Task LoadPurchTask()
        {
            var s = DAL.AppDataBase.GetSetup().Result;
            tasklisttitel = s.ToDoTaskListName;

            var pt = DAL.AppDataBase.GetToDoTasks().ToObservableCollection(); ;
            if (pt != null)
            {
                todotasks = pt;
            }
        }
        #endregion

        #region DataBaseInfos
        private async Task LoadDatabaseInfos()
        {
            await DAL.AppDataBase.SaveLogEntry(LogType.AppInfo, "Load Database Infos");
            _infoM.TotalPicsinDB = "Total Bilder: " + await DAL.AppDataBase.CountPicsInTable();
            _infoM.ViewedPics = "Bereits angezeigt: " + await DAL.AppDataBase.CountPicsInTable(true);
            _infoM.NonViewedPics  = "Fehlen noch: " + await DAL.AppDataBase.CountPicsInTable(false);
            UpdateUI();
        }
        #endregion

        #region Event Handler

        private async void DoSomething()
        {

        }
        #endregion
    }
}
