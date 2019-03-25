using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement; //ApplicationView
using Windows.ApplicationModel.Core; //CoreApplicationViewTitleBar
using Windows.ApplicationModel.Background; //BackgroundTasks
using Windows.Media.Playback; //BackgroundMediaPlayer
using System.Threading.Tasks; //Tasks
using Windows.UI.Popups; //Messagebox MessageDialog
using UwpSqliteDal;
using Windows.UI.Core;
using Windows.Storage;
using System.Collections.ObjectModel;
using AppSettings;
using RWPBGTasks;
using UwpSqLiteDal;
using MSGraph.Response;

namespace HelloWindowsIot
{
    /// <summary>
    /// Settings Page 
    /// </summary>
    public sealed partial class SettingsPage : Page
    {


        private List<TimeObject> ChangeWallpaperTimes { get { return AppSettings.ChangeWallpaperTimeCollection; } }
        private List<TimeObject> SearchPicturesTimes { get { return AppSettings.SearchTimeCollection; } }
        private TimeObject SelectedTimeForChangeWallpaper { get; set; }
        private TimeObject SelectedTimeForSearchPictures { get; set; }

        /// <summary>
        /// Gets or sets the Settingsdata . 
        /// </summary>
        public SettingsViewModel ViewModel { get; set; }


        public SettingsPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();

            //timeComboBox.ItemsSource = ChangeWallpaperTimes;
            //timeBoxForSearchPictures.ItemsSource = SearchPicturesTimes;
        }

        #region Navigate
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dal.SaveLogEntry(LogType.Info, "Navigated To SettingsPage");

            await ViewModel.LoadData();

            //await SelectItemInTimeBoxForSearchPictures();
            //await SelectItemInTimeBoxForChangeWallpaper();

            //UpdateUI();

        }
        #endregion

        /// <summary>
        /// Update the scenario UI.
        /// </summary>
        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                //switchSetWallpaper.IsOn = Settings.ChangeWallpaperTaskRegistered;
                //tooglelogging.IsOn = Dal.GetSetup().EnableLogging;
            });
        }

        /// <summary>
        /// Pre Select Item in ComboBox with Time for Search Pictures
        /// </summary>
        /// <returns></returns>
        private async Task SelectItemInTimeBoxForSearchPictures()
        {
            //try
            //{
            //    var s = await GetSetupConfig();
            //    if (s.IntervalForSearchPictures > 0)
            //    {
            //        int index = SearchPicturesTimes.FindIndex(a => a.TMinutes == s.IntervalForSearchPictures);
            //        if (index >= 0)
            //        {
            //            timeBoxForSearchPictures.SelectedItem = SearchPicturesTimes.ElementAt(index);
            //        }
            //        if ((timeBoxForSearchPictures.SelectedItem == null) || index < 0)
            //            timeBoxForSearchPictures.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        //Select First Entry 
            //        timeBoxForSearchPictures.SelectedIndex = 0;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Dal.SaveLogEntry(LogType.Exception, "Exception in  SelectItemInTimeBoxForSearchPictures() " + ex.Message);
            //}
        }
        /// <summary>
        /// Pre Select Item in ComboBox with Time for Change Wallpaper
        /// </summary>
        /// <returns></returns>
        private async Task SelectItemInTimeBoxForChangeWallpaper()
        {
            //try
            //{
            //    var s = await GetSetupConfig();
            //    if (s.IntervalForChangeWallPaper > 0)
            //    {
            //        int index = ChangeWallpaperTimes.FindIndex(a => a.TMinutes == s.IntervalForChangeWallPaper);
            //        if (index >= 0)
            //        {
            //            timeComboBox.SelectedItem = ChangeWallpaperTimes.ElementAt(index);
            //        }
            //        if ((timeComboBox.SelectedItem == null) || index < 0)
            //            timeComboBox.SelectedIndex = 0;
            //    }
            //    else
            //    {
            //        //Select First Entry 
            //        timeComboBox.SelectedIndex = 0;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Dal.SaveLogEntry(LogType.Exception, "Exception in  SelectItemInTimeBoxForChangeWallpaper() " + ex.Message);
            //}
        }
        #region Tasksample

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachChangeWallpaperProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }


        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachServicingCompleteProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressServicing);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedServicing);
        }



        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var progress = "Progress: " + args.Progress + "%";
            Settings.ChangeWallpaperTaskProgress = progress;
            UpdateUI();

        }


        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgressServicing(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var progress = "Servicing Progress: " + args.Progress + "%";
            Settings.ServicingCompleteTaskProgress = progress;
            UpdateUI();

        }

        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedServicing(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
           {
               //ApplySettings();
           });
        }
        #endregion
        private async void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            //Dal.SaveLogEntry(LogType.Info, "Button ApplySettings Clicked");
            //await Task.Run(()=> ApplySettings());
            ApplySettings();
        }

        private async void ApplySettings()
        {
            Dal.SaveLogEntry(LogType.Info, "ApplySettings...");
            try
            {


                //if (switchSetWallpaper.IsOn)
                //{
                //    Dal.SaveLogEntry(LogType.Info, "Enable Tasks");

                //    if (!Settings.ChangeWallpaperTaskRegistered)
                //    {
                //        var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ChangeWallpaperTaskEntryPoint, Settings.ChangeWallpaperTaskName, Dal.GetTimeIntervalForTask(Settings.ChangeWallpaperTaskName), null);
                //        AttachChangeWallpaperProgressAndCompletedHandlers(task);
                //    }

                //    if (!Settings.SearchPicturesTaskRegistered)
                //    {
                //        var searchPicTask = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                //                                                          Settings.SearchPicturesTaskName,
                //                                                          Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                //                                                          null);
                //    }

                //    UpdateUI();
                //}
                //else
                //{
                //    Dal.SaveLogEntry(LogType.Info, "Disable Tasks");
                //    BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.ChangeWallpaperTaskName);
                //    BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.SearchPicturesTaskName);
                //    UpdateUI();
                //}
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception ex in ApplySettings " + ex.Message);
            }
        }

        private void timeBoxForSearchPictures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////remember time
            //Setup s = Dal.GetSetup();
            //TimeObject timeinterval = timeBoxForSearchPictures.SelectedItem as TimeObject;
            //int minutes = timeinterval.TMinutes;
            //s.IntervalForSearchPictures = timeinterval.TMinutes;
            //Dal.UpdateSetup(s);

            
        }

        private void timeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Setup s = Dal.GetSetup();
            //TimeObject timeinterval = timeComboBox.SelectedItem as TimeObject;
            //int minutes = timeinterval.TMinutes;
            //s.IntervalForChangeWallPaper = timeinterval.TMinutes;
            //Dal.UpdateSetup(s);
        }

        private async void Logging_Click(object sender, RoutedEventArgs e)
        {
            await ApplyLogging();
        }

        private async Task ApplyLogging()
        {
             //Dal.EnableLogging(tooglelogging.IsOn);
        }

        private async Task<Setup> GetSetupConfig()
        {
            return await Dal.GetSetup();
        }

        private void GetSubFolders_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TaskFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var t = cmbTaskFolders.SelectedItem as TaskFolder;
            System.Diagnostics.Debug.WriteLine("New Select: " + t.Id);
        }
    }
}
