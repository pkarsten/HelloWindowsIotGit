﻿using AppSettings;
using HelloWindowsIot;
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
    public sealed class LoadGraphData : IBackgroundTask
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
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting..." + " at " + DateTime.UtcNow.AddHours(Configuration.InitialSetupConfig.EventsOffset));

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
                    await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Background Cost " + BackgroundWorkCost.CurrentBackgroundWorkCost + "in " + taskInstance.Task.Name);
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
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception in Run() Task LoadCalendarEventsAndTasks " + ex.Message);
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
        private async void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;
            _cancelReason = reason;
            await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Background " + sender.Task.Name + " Cancel Requested... ");
        }
        #endregion

        #region Background Task Activity Functions
        //
        // The background task activity.
        //
        private async Task LoadCalendarEventsAndTasks()
        {
            await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Entry in LoadCalendarEventsAndTasks()");

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
                    var s = await DAL.AppDataBase.GetSetup();
                    if (s.EnableCalendarAddon)
                    {
                        await DAL.AppDataBase.DeleteAllCalendarEvents();
                        //
                        // Graphservice for get Calendar Events
                        //

                        if (s.EnableCalendarNextEvents)
                        {
                            IList<CalendarEventItem> nextevents = await graphService.GetCalendarEvents(s.NextEventDays);
                            BGTasksSettings.NextEvents = nextevents.ToObservableCollection();
                            foreach (var o in nextevents)
                            {
                                var ce = new CalendarEvent();
                                //TODO: It seems that sqliteDB all the time saves datetime  as UTC , 
                                //Perhaps add language localisation settings ? 
                                // no way to save it to localtime or other Format? So add here 4 Houts for my timezone 
                                //Winter Time in Düsseldorf Add 1, Summer Time Add 2 , or viceversa? 
                                ce.StartDateTime = o.StartDateTime.dateTime.AddHours(s.EventsOffset); 
                                //TODO: Summertime ? Get here UTC Date ? TODO: Time zone in setup choose? 
                                ce.Subject = o.Subject;
                                if (ce.StartDateTime.Day ==  DateTime.UtcNow.Day)
                                    ce.TodayEvent = true;
                                else
                                    ce.TodayEvent = false;

                                ce.IsAllDay = o.IsAllDay;

                                // TODO: more test for this here, perhaps there are Events that we would see , then don't ignore them 
                                // Problem is when StartTime is between 0:00-02:00 , example: exists an IsAllDay Event on 10.04.19 LocalTime (Begins 0:00, Ends at 11.04.19 0:00)
                                // when the day changes (Localtime) on 0:00 Uhr, then it will list this event as Today Event (because it ends on 11.04) ...
                                if (ce.StartDateTime.Day+1 == DateTime.UtcNow.Day)
                                {
                                    if (ce.IsAllDay)
                                    {
                                        ce.IgnoreEvent = true;
                                    }
                                }
                              
                                //ce.StartDateTime.ToLocalTime();
                                //string us = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", o.StartDateTime.dateTime);
                                //System.Diagnostics.Debug.WriteLine("UTC Time: " + us + " " + o.Subject);
                                //DateTime locDT = o.StartDateTime.dateTime.ToLocalTime();
                                //string strutcStart = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", locDT);
                                //System.Diagnostics.Debug.WriteLine("Loc Time: " + strutcStart + " " + o.Subject);
                                //ce.StartDateTime = new DateTime(locDT.Year,locDT.Month,locDT.Day, locDT.Hour,locDT.Minute,locDT.Second);
                                //ce.StartDateTime = new DateTime(2019, 4, 16, 22, 22, 22);

                                await DAL.AppDataBase.SaveCalendarEvent(ce);
                            }
                        }
                        
                    }

                    if (s.EnablePurchaseTask)
                    {
                        await DAL.AppDataBase.DeleteToDoTask();
                        //Graph Service for get Tasks
                        var mypurchtask = await graphService.GetTasksFromToDoTaskList(s.ToDoTaskListID);

                        if (mypurchtask != null)
                        {
                            foreach (TaskResponse p in mypurchtask)
                            {
                                var pt = new ToDoTask();
                                pt.Subject = p.Subject;
                                pt.BodyText = p.TaskBody.Content;
                                await DAL.AppDataBase.SaveToDoTask(pt);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception  in LoadImageListFromOneDrive() " + ex.Message);
                }
                _progress = 100;
            }
            catch (Exception ex)
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception  in LoadImageListFromOneDrive() " + ex.Message);
            }
            finally
            {
                var settings = ApplicationData.Current.LocalSettings;
                var key = _taskInstance.Task.Name;

                //
                // Write to LocalSettings to indicate that this background task ran.
                //
                settings.Values[key] = (_progress < 100) ? "Canceled with reason: " + _cancelReason.ToString() : "Completed";
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " + DateTime.UtcNow.AddHours(Configuration.InitialSetupConfig.EventsOffset) + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);
            }
        }
        #endregion
    }
}