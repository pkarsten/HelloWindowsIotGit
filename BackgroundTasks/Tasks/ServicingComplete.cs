using AppSettings;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace RWPBGTasks
{
    //
    // A background task always implements the IBackgroundTask interface.
    //
    public sealed class ServicingComplete : IBackgroundTask
    {
        #region variablen
        BackgroundTaskDeferral _deferral = null;
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool _cancelRequested = false;
        IBackgroundTaskInstance _taskInstance = null;

        #endregion
        private static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        //
        // The Run method is the entry point of a background task.
        //
        public async void Run(IBackgroundTaskInstance taskInstance)
        {

            try
            {
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name != taskInstance.Task.Name)
                    {
                        cur.Value.Unregister(true);
                        Dal.SaveLogEntry(LogType.Info, "Unregister BackgroundTask " + taskInstance.Task.Name);
                    }
                }

                //
                // Get the deferral object from the task instance, and take a reference to the taskInstance;
                //
                _deferral = taskInstance.GetDeferral();

                Dal.SaveLogEntry(LogType.Info, "ServicingComplete " + taskInstance.Task.Name + " starting...");

                //
                // Associate a cancellation handler with the background task.
                //
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
                _taskInstance = taskInstance;

                await Task.Run(() => MakeSomeThingsAfterUpdate());
                _deferral.Complete();
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "ServicingComplete in Run() Exception " + ex.Message);
            }
            finally { }
        }

        //
        // Handles background task cancellation.
        //
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;
            _cancelReason = reason;
            Dal.SaveLogEntry(LogType.Info, sender.Task.Name +" is Canceled ");
        }

        private async Task MakeSomeThingsAfterUpdate()
        {
            try
            {
                Dal.DeleteAllLogEntries();
                //
                // Do background task activity for servicing complete.
                //
                uint Progress;
                //((HEAVY WORK ))
                for (Progress = 0; Progress <= 100; Progress += 10)
                {
                    //
                    // If the cancellation handler indicated that the task was canceled, stop doing the task.
                    //
                    if (_cancelRequested)
                    {
                        break;
                    }
                    Dal.SaveLogEntry(LogType.Info, "Background Servicing Progress " + Progress);

                    //
                    // Indicate progress to foreground application.
                    //
                    _taskInstance.Progress = Progress;
                }

                    //await BackgroundTaskConfig.ReRegisterTasks();
                //END HEAVY WORK HERE , RE REGISTER Tasks if needed
                Dal.SaveLogEntry(LogType.Info, "Re Register Tasks) ");
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    //if (cur.Value.Name != Configuration.ServicingCompleteTaskName)
                    //{
                        cur.Value.Unregister(true);
                        Dal.SaveLogEntry(LogType.Info, "Unregister BackgroundTask for RE Register " + cur.Value.Name);
                    //}
                }
                foreach (var tstatus in Dal.GetAllTaskStatus())
                {
                    if ((tstatus.TaskName == Settings.SearchPicturesTaskName) && tstatus.CurrentRegisteredStatus == true)
                    {
                        Settings.SearchPicturesTaskResult = "";
                        var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                               Settings.SearchPicturesTaskName,
                                                                                await Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                                                                               null);
                    }
                    if ((tstatus.TaskName == Settings.ChangeWallpaperTaskName) && tstatus.CurrentRegisteredStatus == true)
                    {
                        var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ChangeWallpaperTaskEntryPoint, Settings.ChangeWallpaperTaskName, await Dal.GetTimeIntervalForTask(Settings.ChangeWallpaperTaskName), null);
                    }

                    if ((tstatus.TaskName == Settings.CreateMessageTaskName) && tstatus.CurrentRegisteredStatus == true)
                    {
                        var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.CreateMessageTaskEntryPoint, Settings.CreateMessageTaskName, await Dal.GetTimeIntervalForTask(Settings.CreateMessageTaskName), null);
                    }

                    if ((tstatus.TaskName == Settings.ServicingCompleteTaskName) && tstatus.CurrentRegisteredStatus == true)
                    {
                        var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ServicingCompleteTaskEntryPoint,
                                                                     Settings.ServicingCompleteTaskName,
                                                                     new SystemTrigger(SystemTriggerType.ServicingComplete, false),
                                                                     null);
                    }
                }



                Dal.SaveLogEntry(LogType.Info, "Reset Log Table after Update to " + GetAppVersion());
                var settings = ApplicationData.Current.LocalSettings;
                var key = _taskInstance.Task.Name;

                //
                // Write to LocalSettings to indicate that this background task ran.
                //
                settings.Values[key] = (Progress < 100) ? "Canceled" : "Completed";

                UwpSqliteDal.BGTask ts = Dal.GetTaskStatusByTaskName(_taskInstance.Task.Name);
                ts.LastTimeRun = DateTime.Now.ToString();
                ts.AdditionalStatus = settings.Values[key].ToString();
                Dal.UpdateTaskStatus(ts);
                Dal.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " + DateTime.Now + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);

            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception in ServicingComplete-MakeSomeThingsAfterUpdate() " + ex.Message);
            }
        }
    }
}
