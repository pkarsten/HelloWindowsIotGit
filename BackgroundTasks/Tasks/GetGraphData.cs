using AppSettings;
using MSGraph;
using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Storage;


namespace RWPBGTasks
{
    public sealed class GetCalendarEventsAndTasks : IBackgroundTask
    {
        #region variablen
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        uint _progress = 0;
        IBackgroundTaskInstance _taskInstance = null;
        #endregion

        #region MainTask
        //============================< MainTask >============================
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                await Dal.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting..." + " at " + DateTime.Now);

                //
                // Get the deferral object from the task instance, and take a reference to the taskInstance;
                //
                _deferral = taskInstance.GetDeferral();

                _taskInstance = taskInstance;

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
                    await Dal.SaveLogEntry(LogType.Info, "Background Cost " + BackgroundWorkCost.CurrentBackgroundWorkCost + "in " + taskInstance.Task.Name);
                }
                else
                {
                    // Do All Work
                    if (_cancelRequested)
                    {
                    }
                }

                await LoadCalendarEventsAndTasks();
            }
            catch (Exception ex)
            {
                await Dal.SaveLogEntry(LogType.Error, "Exception in Run() Task LoadCalendarEventsAndTasks " + ex.Message);
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

        #region Background Task Activity Functions
        //
        // The background task activity.
        //
        private async Task LoadCalendarEventsAndTasks()
        {
            await Dal.SaveLogEntry(LogType.Info, "Entry in LoadCalendarEventsAndTasks()");

            if ((_cancelRequested == false) && (_progress < 100))
            {
            }
            try
            {
                //// Initialize Graph client
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);

                try
                {
                    var s = await Dal.GetSetup();
                    if (s.EnableCalendarAddon)
                    {
                        //Graphservice for get Calendar Events
                        if (s.EnableTodayEvents)
                        {
                            IList<CalendarEventItem> myeventstoday = await graphService.GetTodayCalendarEvents();
                            todayEvents = myeventstoday.ToObservableCollection();
                        }
                        if (s.EnableCalendarNextEvents)
                        {
                            IList<CalendarEventItem> myevents = await graphService.GetCalendarEvents();
                            calendarEvents = myevents.ToObservableCollection();
                        }
                        
                    }

                    if (s.EnablePurchaseTask)
                    {
                        //Graph Service for get Tasks
                        var mypurchtask = await graphService.GetPurchaseTask();
                    }
                    //Settings.DashBoardImage = bitmapimage;
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                _progress = 100;
            }
            catch (Exception ex)
            {
                await Dal.SaveLogEntry(LogType.Error, "Exception  in LoadImageListFromOneDrive() " + ex.Message);
            }
            finally
            {
                await TaskFunctions.ChangeDashBoardBackGroundAsync(false);

                var settings = ApplicationData.Current.LocalSettings;
                var key = _taskInstance.Task.Name;

                //
                // Write to LocalSettings to indicate that this background task ran.
                //
                settings.Values[key] = (_progress < 100) ? "Canceled with reason: " + _cancelReason.ToString() : "Completed";
                //TODO: ??//ERROR =>System.NullReferenceException ?? 
                //BGTask ts = Dal.GetTaskStatusByTaskName(_taskInstance.Task.Name);
                //ts.LastTimeRun = DateTime.Now.ToString();
                //ts.AdditionalStatus = settings.Values[key].ToString();
                //Dal.UpdateTaskStatus(ts);
                await Dal.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " + DateTime.Now + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);
            }
        }
        #endregion
    }
}