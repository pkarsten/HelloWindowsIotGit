﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using RWPBGTasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.System;
using UwpSqliteDal;
using AppSettings;

namespace HelloWindowsIot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class StatusPage : Page
    {
        public StatusPage()
        {
            this.InitializeComponent();
            PageTitle.Text = AppcFuncs.GetLanguage("TitleTaskOverview");
            UpdateUI();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Dal.SaveLogEntry(LogType.Info, "Navigated To StatusPage");
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == Settings.SearchPicturesTaskName)
                {
                    AttachSearchPictureProgressAndCompletedHandlers(task.Value);
                }
                if (task.Value.Name == Settings.ChangeWallpaperTaskName)
                {
                    AttachChangeWallpaperProgressAndCompletedHandlers(task.Value);
                }
                if (task.Value.Name == Settings.ServicingCompleteTaskName)
                {
                    AttachServicingCompleteProgressAndCompletedHandlers(task.Value);
                }
                if (task.Value.Name == Settings.CreateMessageTaskName)
                {
                    AttachCreateMessageProgressAndCompletedHandlers(task.Value);
                }
            }
            UpdateUI();
        }

        /// <summary>
        /// Update the scenario UI.
        /// </summary>
        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                //SearchPicOverviewTitle (get From Resources
                SearchPicOverviewName.Text = "Name: " + Settings.SearchPicturesTaskName;
                SearchPicOverviewStatus.Text = "Status: " + BackgroundTaskConfig.GetBackgroundTaskStatus(Settings.SearchPicturesTaskName);
                if (!String.IsNullOrEmpty(Dal.GetTaskStatusByTaskName(Settings.SearchPicturesTaskName).LastTimeRun))
                    SearchPicOverviewLastRun.Text = AppcFuncs.GetLanguage("txtLastRun") + " " + Dal.GetTaskStatusByTaskName(Settings.SearchPicturesTaskName).LastTimeRun;
                else SearchPicOverviewLastRun.Text = "";
                SearchPicOverviewProgress.Text = Settings.SearchPicturesTaskProgress;
                SearchPicOverviewResult.Text = Settings.SearchPicturesTaskResult;


                //ChangeWPOverviewTitle from resources
                ChangeWPOverviewName.Text = "Name: " + Settings.ChangeWallpaperTaskName;
                ChangeWPOverviewStatus.Text = "Status: " + BackgroundTaskConfig.GetBackgroundTaskStatus(Settings.ChangeWallpaperTaskName);
                if (!String.IsNullOrEmpty(Dal.GetTaskStatusByTaskName(Settings.ChangeWallpaperTaskName).LastTimeRun))
                    ChangeWPOverviewLastRun.Text = AppcFuncs.GetLanguage("txtLastRun") + " " + Dal.GetTaskStatusByTaskName(Settings.ChangeWallpaperTaskName).LastTimeRun;
                else
                    ChangeWPOverviewLastRun.Text = "";
                ChangeWPOverviewProgress.Text = Settings.ChangeWallpaperTaskProgress;
                ChangeWPOverviewResult.Text = Settings.ChangeWallpaperTaskResult;

                //CreateMessageTask 
                CreateMessageOverviewName.Text = "Name: " + Settings.CreateMessageTaskName;
                CreateMessageOverviewStatus.Text = "Status: " + BackgroundTaskConfig.GetBackgroundTaskStatus(Settings.CreateMessageTaskName);
                if (!String.IsNullOrEmpty(Dal.GetTaskStatusByTaskName(Settings.CreateMessageTaskName).LastTimeRun))
                    CreateMessageOverviewLastRun.Text = AppcFuncs.GetLanguage("txtLastRun") + " " + Dal.GetTaskStatusByTaskName(Settings.CreateMessageTaskName).LastTimeRun;
                else
                    CreateMessageOverviewLastRun.Text = "";
                CreateMessageOverviewProgress.Text = Settings.CreateMessageTaskProgress;
                CreateMessageOverviewResult.Text = Settings.CreateMessageTaskResult;

                //ServiceOverviewTitle from resources
                ServiceOverviewName.Text = Settings.ServicingCompleteTaskName;
                ServiceOverviewStatus.Text = "Status: " + BackgroundTaskConfig.GetBackgroundTaskStatus(Settings.ServicingCompleteTaskName);

                if (!String.IsNullOrEmpty(Dal.GetTaskStatusByTaskName(Settings.ServicingCompleteTaskName).LastTimeRun))
                    ServiceOverviewLastRun.Text = AppcFuncs.GetLanguage("txtLastRun") + " " + Dal.GetTaskStatusByTaskName(Settings.ServicingCompleteTaskName).LastTimeRun;
                else ServiceOverviewLastRun.Text = "";
                ServiceOverviewProgress.Text = Settings.ServicingCompleteTaskProgress;
                ServiceOverviewResult.Text = Settings.ServicingCompleteTaskResult;
                
            });
        }
        #region SearchPictureTaskEventhandler
       
        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachSearchPictureProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressSearchPictures);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedSearchPictures);
        }
        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedSearchPictures(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }
        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgressSearchPictures(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var progress = "Progress: " + args.Progress + "%";
                Settings.SearchPicturesTaskProgress = progress;
                UpdateUI();
            });
        }
        #endregion

        #region Change Wallpaper eventhandler
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
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }
        #endregion

        #region Servicing Complete eventhandler
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
        private async void OnCompletedServicing(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }

        #endregion

        #region CreateMessageTask Eventhandler
        
        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachCreateMessageProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressCreateMessage);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedCreateMessage);
        }
        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedCreateMessage(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }
        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgressCreateMessage(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var progress = "Progress: " + args.Progress + "%";
                Settings.CreateMessageTaskProgress = progress;
                UpdateUI();
            });
        }
        #endregion

        #region Un/Register Buttons
        /// <summary>
        /// Register a SampleBackgroundTask.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterSearchPictureTask(object sender, RoutedEventArgs e)
        {
            var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                          Settings.SearchPicturesTaskName,
                                                                          Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                                                                          null);
            AttachSearchPictureProgressAndCompletedHandlers(task);
            UpdateUI();
        }

        /// <summary>
        /// Unregister a SampleBackgroundTask.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnRegisterSearchPictureTask(object sender, RoutedEventArgs e)
        {
            BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.SearchPicturesTaskName);
            UpdateUI();
        }
        /// <summary>
        /// Register a ChangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCHangeWPTask(object sender, RoutedEventArgs e)
        {
            var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ChangeWallpaperTaskEntryPoint,
                                                                          Settings.ChangeWallpaperTaskName,
                                                                          Dal.GetTimeIntervalForTask(Settings.ChangeWallpaperTaskName),
                                                                          null);
            AttachChangeWallpaperProgressAndCompletedHandlers(task);
            UpdateUI();
        }

        /// <summary>
        /// Unregister a CHangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnRegisterCHangeWPTask(object sender, RoutedEventArgs e)
        {
            BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.ChangeWallpaperTaskName);
            UpdateUI();
        }
        /// <summary>
        /// Register a ChangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCreateMessageTask(object sender, RoutedEventArgs e)
        {
            var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.CreateMessageTaskEntryPoint,
                                                                          Settings.CreateMessageTaskName,
                                                                          Dal.GetTimeIntervalForTask(Settings.CreateMessageTaskName),
                                                                          null);
            AttachCreateMessageProgressAndCompletedHandlers(task);
            UpdateUI();
        }

        /// <summary>
        /// Unregister a CHangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnRegisterCreateMessageTask(object sender, RoutedEventArgs e)
        {
            BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.CreateMessageTaskName);
            UpdateUI();
        }

        /// <summary>
        /// Register a ChangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterServiceTask(object sender, RoutedEventArgs e)
        {
            var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ServicingCompleteTaskEntryPoint,
                                                                          Settings.ServicingCompleteTaskName,
                                                                          Dal.GetTimeIntervalForTask(Settings.ServicingCompleteTaskName),
                                                                          null);
            AttachServicingCompleteProgressAndCompletedHandlers(task);
            UpdateUI();
        }

        /// <summary>
        /// Unregister a CHangeWP.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnRegisterServiceTask(object sender, RoutedEventArgs e)
        {
            BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.ServicingCompleteTaskName);
            UpdateUI();
        }
        #endregion
    }
}
