using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using UwpSqliteDal;
using AppSettings;
using System.Linq;

namespace RWPBGTasks
{
    public static class BackgroundTaskConfig
    {


        // Because WinRt can use, but can't return Task<T> h ttp://dotnetbyexample.blogspot.de/2014/11/returning-task-from-windows-runtime.html
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CallFromBackgroundTask">Define if Function was called from Background Task or Startet by Manual Button Click in UI </param>
        /// <returns></returns>
        public static IAsyncOperation<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            return InternalRegisterBackgroundTaskAsync(taskEntryPoint, name, trigger, condition).AsAsyncOperation();
        }

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        private static async Task<BackgroundTaskRegistration> InternalRegisterBackgroundTaskAsync(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            Dal.SaveLogEntry(LogType.Info, "Register BackgroundTask " + name);
            BackgroundExecutionManager.RemoveAccess();

            var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();

            if (hasAccess == BackgroundAccessStatus.DeniedByUser)
            {
                Dal.SaveLogEntry(LogType.Error, "BackgroundAccessStatus.Denied " + name);
                return null;
            }

            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);
            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
                Dal.SaveLogEntry(LogType.Info, "CancelOnConditionLoss " + name);
            }

            BackgroundTaskRegistration task = builder.Register();
            Dal.SaveLogEntry(LogType.Info, "Background Task " + name + " Registered " + " at " + DateTime.Now);
            UpdateBackgroundTaskRegistrationStatus(name, true);

            return task;
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks(String name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                    Dal.SaveLogEntry(LogType.Info, "Unregister " + name);
                }
            }

            UpdateBackgroundTaskRegistrationStatus(name, false);
        }


        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        public static void UpdateBackgroundTaskRegistrationStatus(String name, bool registered)
        {
            var til = Settings.ListBgTasks.Where(g => g.Name == name).FirstOrDefault();
            til.Registered = registered;

            
            var ts = Dal.GetTaskStatusByTaskName(name);
            if (ts != null)
            {
                var settings = ApplicationData.Current.LocalSettings;
                string additionalStatus = "";
                if (settings.Values.ContainsKey(name))
                {
                    additionalStatus = settings.Values[name].ToString();
                }

                ts.CurrentRegisteredStatus = registered;
                ts.AdditionalStatus = additionalStatus;
                Dal.UpdateTaskStatus(ts);
                Dal.SaveLogEntry(LogType.Info, string.Format("Update {0} status registered: {1} ", name, registered));
            }
        }



        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;
            var ts = Settings.ListBgTasks.Where(g => g.Name == name).FirstOrDefault();
            registered = ts.Registered;

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static bool GetBackgroundRegisteredTaskStatus(String name)
        {
            var registered = false;

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    registered = true;
                }
            }
            return registered;
        }

        /// <summary>
        /// Determine if task with given name requires background access.
        /// </summary>
        /// <param name="name">Name of background task to query background access requirement.</param>
        public static bool TaskRequiresBackgroundAccess(String name)
        {
            var ts = Settings.ListBgTasks.Where(g => g.Name == name).FirstOrDefault();
            if (ts.Name == name)
            { 
                Dal.SaveLogEntry(LogType.Info, name + "requires Background access");
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IAsyncOperation<bool> CheckForRegisteredTasks()
        {
            return InternalCheckForRegisteredTasks().AsAsyncOperation();
        }

        /// <summary>
        /// Check if Our Tasks are registered and Update the Registration Status
        /// </summary>
        private static async Task<bool> InternalCheckForRegisteredTasks()
        {
            // First Check if Task Table is Filled 
            foreach(BGTaskModel bgt in Settings.ListBgTasks)
            {
                var ts = Dal.GetTaskStatusByTaskName(bgt.Name);
                if (ts == null)
                {
                    UwpSqliteDal.BGTask t = new UwpSqliteDal.BGTask();
                    t.TaskName = ts.TaskName;
                    t.CurrentRegisteredStatus = false;
                    t.LastTimeRun = "";
                    t.AdditionalStatus = "";
                    Dal.UpdateTaskStatus(t);
                }
            }

            // Look for running Tasks and Set the Status 
            foreach (BGTaskModel bgt in Settings.ListBgTasks)
            {
                UpdateBackgroundTaskRegistrationStatus(bgt.Name, true);
            }


            // Check if must Register Background Tasks
            foreach (BGTaskModel bgt in Settings.ListBgTasks)
            {
                switch (bgt.Name)
                {
                    case Settings.ServicingCompleteTaskName:
                        // Normally Servicing Complete  Task must always Registered because it makes Some Things when App will be an update
                        // TODO: Servicing Complete not needed in HellWindowsIOT App so set RegisterSystemTriggerBackgroundTasks = false in Settings:
                        //SystemTrigger
                        if (Settings.RegisterSystemTriggerBackgroundTasks == true)
                        {
                            var task = await RegisterBackgroundTask(bgt.EntryPoint, bgt.Name, new SystemTrigger(SystemTriggerType.ServicingComplete, false), null);
                        }
                        break;
                    default:
                        // Register Time Triggered Tasks if is not registered but Status iN DB say it must be registered or if Settings Register All Tasks 
                        if (((Dal.GetTaskStatusByTaskName(bgt.Name).CurrentRegisteredStatus == true) && bgt.Registered == false) || (Settings.RegisterAllBackgroundTasks == true))
                        {
                            // TimeTrigger
                            var task = await RegisterBackgroundTask(bgt.EntryPoint, bgt.Name, Dal.GetTimeIntervalForTask(bgt.Name), null);
                        }
                        break;
                }
            }

            return true;

        }
    }
}
