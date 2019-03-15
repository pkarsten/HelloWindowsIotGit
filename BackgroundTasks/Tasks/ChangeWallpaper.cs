using System;
using System.Diagnostics;
using System.Threading;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;

namespace RWPBGTasks
{
    public sealed class ChangeWallpaper : IBackgroundTask
    {
        #region variablen
        BackgroundTaskDeferral _deferral = null;
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        #endregion

        #region MainTask
        //============================< MainTask >============================
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                Dal.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting..." + " at " + DateTime.Now);

                //
                // Get the deferral object from the task instance, and take a reference to the taskInstance;
                //
                _deferral = taskInstance.GetDeferral();

                //
                // Associate a cancellation handler with the background task.
                //
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

                //
                // Query BackgroundWorkCost
                // Guidance: If BackgroundWorkCost is high, then perform only the minimum amount
                // of work in the background task and return immediately.
                // https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.background.backgroundworkcostvalue
                var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values["BackgroundWorkCost"] = cost.ToString();
               
                if (BackgroundWorkCost.CurrentBackgroundWorkCost != BackgroundWorkCostValue.Low)
                {
                    //Do less things if Backgroundcost is high or medium
                    Dal.SaveLogEntry(LogType.Info, "Background Cost " + BackgroundWorkCost.CurrentBackgroundWorkCost + "in " + taskInstance.Task.Name);
                }
                else
                {
                    // Do All Work
                    if (_cancelRequested)
                    {
                    }
                }
                
                await TaskFunctions.ChangeWallpaperAsync(true);
                BGTask ts = Dal.GetTaskStatusByTaskName(taskInstance.Task.Name);
                ts.LastTimeRun = DateTime.Now.ToString();
                Dal.UpdateTaskStatus(ts);
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception in Run() Task ChangeWallpeper " + ex.Message);
            }

            finally
            {
                if (_deferral != null)
                {
                // Inform the system that the task is finished.
                _deferral.Complete();
               // No Code will execute after Deferral is Complete
                }
            }
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
            Dal.SaveLogEntry(LogType.Error, "Background " + sender.Task.Name + " Cancel Requested... ");
        }
        #endregion
    }
}
