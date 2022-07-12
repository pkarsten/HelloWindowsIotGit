using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppSettings;
using SQLite;
using Windows.ApplicationModel.Background;
using System.Diagnostics;
using MSGraph.Response;
using MSGraph;


namespace UwpSqliteDal
{
    public class DAL
    {

        #region fields
        static DAL database;
        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;
        #endregion;
        
        #region Properties
        public static DAL AppDataBase
        {
            get
            {
                if (database == null)
                {
                    database = new DAL();
                    System.Diagnostics.Debug.WriteLine("Create New Database !?!?!? ");
                }
                return database;
            }
        }

        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Configuration.DatabasePath, Configuration.Flags);
        });
        #endregion

        #region Constructor
        public DAL()
        {
            InitializeAsync().SafeFireAndForget(false);
            Database.EnableWriteAheadLoggingAsync();
        }
        #endregion

        public async Task InitializeAsync()
        {
            if (!initialized)
            {
               
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(FavoritePic).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(FavoritePic)).ConfigureAwait(false);
                    initialized = true;
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(LogEntry).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(LogEntry)).ConfigureAwait(false);
                    initialized = true;
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(BGTask).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(BGTask)).ConfigureAwait(false);
                    initialized = true;
                }
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(Setup).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Setup)).ConfigureAwait(false);
                    initialized = true;
                }
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(PicFilter).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(PicFilter)).ConfigureAwait(false);
                    initialized = true;
                }


                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(CalendarEvent).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(CalendarEvent)).ConfigureAwait(false);
                    initialized = true;
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(ToDoTask).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ToDoTask)).ConfigureAwait(false);
                    initialized = true;
                }
                
                await CheckSetupData();
                CheckPicFilterData();
            }
        }

        #region Setup
        /// <summary>
        /// Check if must save initial Setup Data
        /// </summary>
        public async Task CheckSetupData()
        {

            if (Database.Table<Setup>().FirstOrDefaultAsync().Result == null) 
            {
                //Save Initial Setup Data if Table hasn't entry 
                Setup initSetup = Configuration.InitialSetupConfig;
                await Database.InsertAsync(initSetup);
            }
        }

        /// <summary>
        /// Get Current Setup COnfig saved in Database
        /// </summary>
        /// <returns></returns>
        public async Task<Setup> GetSetup()
        {
            Setup sconfig = new Setup();
            try
            {
                sconfig = Database.Table<Setup>().FirstOrDefaultAsync().Result;

            }
            catch (Exception ex)
            {
                await SaveLogEntry(LogType.Exception, "GetSetup() Exception: " + ex.Message);
            }
            return sconfig;
        }

        /// <summary>
        /// Update Setup Config Data in Table 
        /// </summary>
        /// <param name="set"></param>
        public async Task UpdateSetup(Setup set)
        {
            try
            {
                await Database.UpdateAsync(set);
                await SaveLogEntry(LogType.Info, "Setup Config Updated");
            }
            catch (Exception ex)
            {
                await SaveLogEntry(LogType.Exception, "UpdateSetup() Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// En/Disable Logging 
        /// </summary>
        /// <param name="enable"></param>
        public async Task EnableLogging(bool enable)
        {
            try
            {
                Setup sconfig = await GetSetup();
                sconfig.EnableLogging = enable;
                
                if (enable == false)
                        await SaveLogEntry(LogType.Info, "Disable Logging");
                    
                await Database.UpdateAsync(sconfig);
                    
                if (enable == true)
                        await SaveLogEntry(LogType.Info, "Enable logging ");
            }
            catch (Exception ex)
            {
                await SaveLogEntry(LogType.Exception, "UpdateSetup() Exception: " + ex.Message);
            }
        }

        public async Task<TimeTrigger> GetTimeIntervalForTask(string taskname)
        {
            Setup s = await GetSetup();
            uint minutesForTrigger = 15;

            switch (taskname)
            {
                case BGTasksSettings.LoadGraphDataTaskName:
                    minutesForTrigger = (uint)s.IntervalForLoadCalendarAndTasksInterval;
                    break;
                case BGTasksSettings.LoadImagesFromOneDriveTaskName:
                    minutesForTrigger = (uint)s.IntervalForLoadCalendarAndTasksInterval;
                    break;
                default:
                    minutesForTrigger = 15;
                    break;
            }
            return new TimeTrigger(minutesForTrigger, false);
        }
        #endregion

        #region PicFilter
        /// <summary>
        /// Check if must save initial Pic Filter Data
        /// </summary>
        private async void CheckPicFilterData()
        {
            PicFilter pfic;
            try
            {
                pfic = Database.Table<PicFilter>().FirstOrDefaultAsync().Result;
                //Save Initial Setup Data if Table hasn't entry 
                if (pfic == null)
                {
                    PicFilter initPicFilter = Configuration.InitialPicFilterConfig;
                    await Database.InsertAsync(initPicFilter);
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Get Current Pic Filter Config saved in Database
        /// </summary>
        /// <returns></returns>
        public async Task<PicFilter> GetPicFilter()
        {
            PicFilter picfilterconfig = new PicFilter();
            try
            {
                picfilterconfig = Database.Table<PicFilter>().Where(i => i.Id == 1).FirstOrDefaultAsync().Result;

            }
            catch (Exception ex)
            {
                await SaveLogEntry(LogType.Exception, "PicFilter() Exception: " + ex.Message);
            }
            return picfilterconfig;
        }

        /// <summary>
        /// Update PicFilter Config Data in Table 
        /// </summary>
        /// <param name="set"></param>
        public void UpdatePicFilterConfig(PicFilter set)
        {
            try
            {
                    Database.UpdateAsync(set);
                    SaveLogEntry(LogType.Info, "PicFilter  Config Updated");
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "UpdatePicFilterConfig() Exception: " + ex.Message);
            }
        }
        #endregion

        #region pictures
        public async void DeletePicture(FavoritePic pic)
        {
                System.Diagnostics.Debug.WriteLine(" ");
                System.Diagnostics.Debug.WriteLine("DELETE DeletePicture(FavoritePic pic)");
                System.Diagnostics.Debug.WriteLine(" ");
                // SQL Syntax:
                await Database.QueryAsync<FavoritePic>("DELETE FROM FavoritePic WHERE Id = ?",pic.Id);
        }

        public  async Task DeleteAllPictures()
        {
            System.Diagnostics.Debug.WriteLine(" ");
            System.Diagnostics.Debug.WriteLine("DELETE DeleteAllPictures");
            System.Diagnostics.Debug.WriteLine(" ");

            // SQL Syntax:
            //await Database.QueryAsync<FavoritePic>("DELETE FROM FavoritePic");
            await Database.DeleteAllAsync<FavoritePic>();
        }

        public void DelIndefinablePics()
        {
            //TODO: db.Execute("DELETE FROM FavoritePic WHERE Status = ?","");
            UpdateAllPicStatus();
        }

        public async void UpdateAllPicStatus()
        {
                Database.QueryAsync<FavoritePic>("UPDATE FavoritePic SET Status=?", "");
                await SaveLogEntry(LogType.Info, "Set Favorite Pics status = empty");

        }

        public async Task<IList<FavoritePic>> GetAllPictures()
        {
            IList<FavoritePic> models;

            models = await Database.Table<FavoritePic>().ToListAsync();
               
            return models;
        }

        public Task<int> CountAllPictures()
        {
            return Database.Table<FavoritePic>().CountAsync();
        }

        public async void CheckForViewedPictures()
        {
            try
            {
                var viewedPics =  Database.Table<FavoritePic>().Where(v => v.Viewed == false).ToListAsync().Result;
                if (viewedPics.Count == 0)
                {
                    var upPics = Database.Table<FavoritePic>().Where(v => v.Viewed == true).ToListAsync().Result;

                    foreach (FavoritePic p in upPics)
                    {
                        p.Viewed = false;
                        await SavePicture(p);
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Error, "Exception in CheckForViewed Pictures " + ex.Message);
            }

        }

        public FavoritePic GetPictureById(int Id)
        {
            FavoritePic m = null;
            try
            {
                 m = Database.Table<FavoritePic>().Where(i => i.Id == Id).FirstOrDefaultAsync().Result;

            }
            catch (Exception ex) { }

            return m;
        }

        public FavoritePic GetPictureByOneDriveId(string id)
        {
            FavoritePic m = Database.Table<FavoritePic>().Where(i => i.OneDriveId == id).FirstOrDefaultAsync().Result;

            return m;
        }

        public async Task SavePicture(FavoritePic pic)
        {
                    if (pic.Id == 0)
                    {
                        // New
                        await Database.InsertAsync(pic);
                    }
                    else
                    {
                        // Update
                        await Database.UpdateAsync(pic);
                    }
        }



        #endregion

        #region CalendarEvents
        public async Task SaveCalendarEvent(CalendarEvent ce)
        {
                    if (ce.Id == 0)
                    {
                        // New
                        try
                        {
                            await Database.InsertAsync(ce);
                        }
                        catch (SQLiteException sqex)
                        {
                            System.Diagnostics.Debug.WriteLine("sex " + sqex.Message);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }

                    }
                    else
                    {
                        // Update
                        await Database.UpdateAsync(ce);
                    }
        }
        public async Task DeleteAllCalendarEvents()
        {
            await Database.DeleteAllAsync<CalendarEvent>();
        }
        public IList<CalendarEvent> GetTodayEvents()
        {
            IList<CalendarEvent> models;

            models = Database.Table<CalendarEvent>().Where(c => c.TodayEvent == true && c.IgnoreEvent == false).OrderBy(u => u.StartDateTime).ToListAsync().Result;


            return models;
        }
        public IList<CalendarEvent> GetNextEvents()
        {
            IList<CalendarEvent> models =null;

            try
            {
                models = Database.Table<CalendarEvent>().Where(c => c.TodayEvent == false && c.IgnoreEvent == false).OrderBy(u => u.StartDateTime).ToListAsync().Result;

            }
            catch(Exception ex)
            {

            }
            return models;
        }
        #endregion

        #region ToDoTask
        public async Task SaveToDoTask(ToDoTask obj)
        {
                if (obj.Id == 0)
                {
                    // New
                    await Database.InsertAsync(obj);
                }
                else
                {
                    // Update
                    await Database.UpdateAsync(obj);
                }
        }
        public async Task DeleteToDoTask()
        {
            await Database.DeleteAllAsync<ToDoTask>();
        }
        public IList<ToDoTask> GetToDoTasks()
        {
            IList<ToDoTask> taskList = null;
            taskList = Database.Table<ToDoTask>().ToListAsync().Result;
            return taskList;
        }
        #endregion

        #region DatabaseInfos
        public async Task<int> CountPicsInTable()
        {
            int pics = 0;

            pics = Database.Table<FavoritePic>().ToListAsync().Result.Count;
            return pics;
        }

        public Task<int> CountPicsInTable(bool viewed)
        {
            if (viewed == true)
                return  Database.Table<FavoritePic>().Where(v => v.Viewed == true).CountAsync();
            else 
                 return Database.Table<FavoritePic>().Where(v => v.Viewed == false).CountAsync();
        }
        #endregion

        #region BGTasks
        public async void DeleteAllTaskStatus()
        {
            try
            {
                await Database.DeleteAllAsync<TaskStatus>();
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Error, "Exception in DeleteAllTaskStatus " + ex.Message);
            }
            finally
            {
                SaveLogEntry(LogType.Info, "All Log entries deleted");
            }
        }

        public BGTask GetTaskStatusByTaskName(string tName)
        {
            BGTask t = Database.Table<BGTask>().Where(p => p.TaskName == tName).FirstOrDefaultAsync().Result;

                return t;
        }

        public string GetTimeFromLastRun(string name)
        {
            var ts = GetTaskStatusByTaskName(name);
            return ts.LastTimeRun;
        }

        public async void UpdateTaskStatus(BGTask ts)
        {
            try
            {
                    // New
                    if (GetTaskStatusByTaskName(ts.TaskName) != null)
                    {
                        await Database.UpdateAsync(ts);
                        SaveLogEntry(LogType.Info, "DB Update TaskStatus " + ts.TaskName + " Current Status: " + ts.CurrentRegisteredStatus);
                    }
                    else
                    {
                        await Database.InsertAsync(ts);
                        SaveLogEntry(LogType.Info, "DB Save TaskStatus " + ts.TaskName + " Current Status: " + ts.CurrentRegisteredStatus);
                    }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "Exception in UpdateTaskStatus " + ex.Message);
            }
            finally
            {

            }
        }

        public BGTask SetCurrentRegistrationStatus(string taskname, bool registered)
        {
            BGTask ts = GetTaskStatusByTaskName(taskname);
            ts.CurrentRegisteredStatus = registered;
            return ts;
        }

        public IList<BGTask> GetAllTaskStatus()
        {
            IList<BGTask> mlist;

            mlist = Database.Table<BGTask>().ToListAsync().Result;
            return mlist;
        }


        #endregion

        #region MS Graph
        public FavoritePic GetRandomInfoItemResponse()
        {
            FavoritePic m = Database.QueryAsync<FavoritePic>("SELECT * FROM FavoritePic WHERE (Viewed == false) AND  (DownloadedFromOneDrive == TRUE) ORDER BY  Random()").Result.FirstOrDefault();
            
            if (m == null)
                return Database.Table<FavoritePic>().FirstOrDefaultAsync().Result;  
            else 
                return m;
        }


        public async Task<IList<TaskFolder>> GetTaskFolderFromGraph()
        {

            Exception error = null;
            IList<TaskFolder> folders = null;

            //// Initialize Graph client
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);

            try
            {
                folders = await graphService.GeTaskFolders();
                foreach (TaskFolder f in folders)
                {
                    System.Diagnostics.Debug.WriteLine("Name: " + f.Name + " - Id: " + f.Id);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (error != null)
                {
                    SaveLogEntry(LogType.Error, error.Message);
                }
            }

            return folders;
        }


        /// <summary>
        /// Gets a List of Tasks from MS Graph in given TaskFolder
        /// </summary>
        /// <returns></returns>
        public async Task<IList<TaskResponse>> GetTasksInFolder(string taskfolderId)
        {

            Exception error = null;
            IList<TaskResponse> tasks = null;

            //// Initialize Graph client
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);

            try
            {
                tasks = await graphService.GetTasksFromToDoTaskList(taskfolderId);
                foreach (TaskResponse t in tasks)
                {
                    System.Diagnostics.Debug.WriteLine("Name: " + t.Subject + " - Id: " + t.Id);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (error != null)
                {
                    SaveLogEntry(LogType.Error, error.Message);
                }
            }

            return tasks;
        }

        #endregion

        #region logEntries
        public IList<LogEntry> GetAllLogs()
        {
            IList<LogEntry> logs;

            logs = Database.Table<LogEntry>().OrderByDescending(d => d.Id).ToListAsync().Result;
            return logs;
        }
        public IList<LogEntry> GetLatestXLogs(int x)
        {
            IList<LogEntry> logs;

            logs = Database.Table<LogEntry>().OrderByDescending(d => d.Id).Take(x).ToListAsync().Result;
            return logs;
        }

        public async void DeleteAllLogEntries()
        {
            try
            {
                await Database.DeleteAllAsync<LogEntry>();

            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Error, "Exception " + ex.Message);
            }
            finally
            {
                SaveLogEntry(LogType.Info, "All Log entries deleted");
            }
        }

        public async Task SaveLogEntry(LogType ltype, string logDescription)
        {

            // 
            // CHeck when Must Save Log Entry 
            //
            Setup n = Database.Table<Setup>().FirstOrDefaultAsync().Result;

#if DEBUG
            n.EnableLogging = true;
#endif
            if ((ltype == LogType.Error) || (ltype == LogType.Exception) || (n.EnableLogging == true) || ltype == LogType.AppInfo)
            {
                try
                {
                    if (ltype == LogType.AppInfo)
                        ltype = LogType.Info;
                    LogEntry lentry = new LogEntry();
                    lentry.LogType = ltype.ToString();
                    lentry.Description = logDescription;
                    lentry.LogEntryDate = DateTime.UtcNow.AddHours(Configuration.InitialSetupConfig.EventsOffset).ToString();
                    // Create a new connection
                    
                        // New
                        //Database.InsertAsync(lentry);
                        System.Diagnostics.Debug.WriteLine("Log: " + lentry.LogType + " " + logDescription);
                }
                catch (Exception ex)
                {
                    //SaveLogEntry(LogType.Error, "Exception in SaveLogEntry() " + ex.Message);
                    System.Diagnostics.Debug.WriteLine(LogType.Error, "Exception in SaveLogEntry() " + ex.Message);
                }
            }

        }
        #endregion

        /*public Task<List<TodoItem>> GetItemsAsync()
        {
            return Database.Table<TodoItem>().ToListAsync();
        }

        public Task<List<TodoItem>> GetItemsNotDoneAsync()
        {
            return Database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        }

        public Task<TodoItem> GetItemAsync(int id)
        {
            return Database.Table<TodoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(TodoItem item)
        {
            if (item.ID != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(TodoItem item)
        {
            return Database.DeleteAsync(item);
        }*/


    }
}