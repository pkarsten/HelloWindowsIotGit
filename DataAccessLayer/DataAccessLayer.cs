using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using SQLite;
using SQLite.Net.Platform.WinRT;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using SQLite.Net;
using AppSettings;
using MSGraph.Response;
using MSGraph;

namespace UwpSqliteDal
{
    /// <summary>
    /// Data Access Layer for Save in Sqlite DB 
    /// </summary>
    public static class Dal
    {
        private static string dbPath = string.Empty;

        #region Database Connection and Create
        /// <summary>
        /// Path where SQLITE DB will saved 
        /// </summary>
        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(dbPath))
                {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Configuration.DatabaseName);
                }

                return dbPath;
            }
        }

        private static SQLiteConnection DbConnection
        {
            get
            {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }

        /// <summary>
        /// Creates DB and Tables if not existent
        /// </summary>
        public static void CreateDatabase()
        {
            // Create a new connection
            using (var db = DbConnection)
            {
                // Create the tables if it does not exist
                var c = db.CreateTable<FavoritePic>();
                var info = db.GetMapping(typeof(FavoritePic));

                var l = db.CreateTable<LogEntry>();
                var linfo = db.GetMapping(typeof(LogEntry));

                var ts = db.CreateTable<BGTask>();
                var tsinfo = db.GetMapping(typeof(BGTask));

                var s = db.CreateTable<Setup>();
                var sinfo = db.GetMapping(typeof(Setup));

                var pf = db.CreateTable<PicFilter>();
                var pfinfo= db.GetMapping(typeof(PicFilter));

                var ms = db.CreateTable<Message>();
                var msinfo = db.GetMapping(typeof(Message));

            }
            CheckSetupData();
            CheckPicFilterData();
        }
        #endregion

        #region PicFilter
        /// <summary>
        /// Check if must save initial Pic Filter Data
        /// </summary>
        private static void CheckPicFilterData()
        {
            PicFilter pfic;
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                pfic = (from p in db.Table<PicFilter>() select p).FirstOrDefault();

                //Save Initial Setup Data if Table hasn't entry 
                if (pfic == null)
                {
                    PicFilter initPicFilter = Configuration.InitialPicFilterConfig;
                    db.Insert(initPicFilter);
                }
            }
        }

        /// <summary>
        /// Get Current Pic Filter Config saved in Database
        /// </summary>
        /// <returns></returns>
        public static PicFilter GetPicFilter()
        {
            PicFilter picfilterconfig = new PicFilter();
            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    picfilterconfig = (from p in db.Table<PicFilter>() where p.Id == 1 select p).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "PicFilter() Exception: " + ex.Message);
            }
            return picfilterconfig;
        }

        /// <summary>
        /// Update PicFilter Config Data in Table 
        /// </summary>
        /// <param name="set"></param>
        public static void UpdatePicFilterConfig(PicFilter set)
        {
            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    db.Update(set);
                    SaveLogEntry(LogType.Info, "PicFilter  Config Updated");
                }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "UpdatePicFilterConfig() Exception: " + ex.Message);
            }
        }
        #endregion


        #region Setup
        /// <summary>
        /// Check if must save initial Setup Data
        /// </summary>
        private static void CheckSetupData()
        {
            Setup sconfig;
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                sconfig = (from p in db.Table<Setup>() select p).FirstOrDefault();

                //Save Initial Setup Data if Table hasn't entry 
                if (sconfig == null)
                {
                    Setup initSetup = Configuration.InitialSetupConfig;
                    db.Insert(initSetup);
                }
            }
        }

        /// <summary>
        /// Get Current Setup COnfig saved in Database
        /// </summary>
        /// <returns></returns>
        public static Setup GetSetup()
        {
            Setup sconfig = new Setup();
            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    sconfig = (from p in db.Table<Setup>() where p.Id == 1 select p).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "GetSetup() Exception: " + ex.Message);
            }
            return sconfig;
        }

        /// <summary>
        /// Update Setup Config Data in Table 
        /// </summary>
        /// <param name="set"></param>
        public static void UpdateSetup(Setup set)
        {
            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    db.Update(set);
                    SaveLogEntry(LogType.Info, "Setup Config Updated");
                }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "UpdateSetup() Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// En/Disable Logging 
        /// </summary>
        /// <param name="enable"></param>
        public static void EnableLogging(bool enable)
        {
            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    Setup sconfig = GetSetup();
                    sconfig.EnableLogging = enable;
                    if (enable == false)
                        SaveLogEntry(LogType.Info, "Disable Logging");
                    db.Update(sconfig);
                    if (enable == true)
                        SaveLogEntry(LogType.Info, "Enable logging ");
                }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Exception, "UpdateSetup() Exception: " + ex.Message);
            }
        }

        public static TimeTrigger GetTimeIntervalForTask(string taskname)
        {
            Setup s = GetSetup();
            uint minutesForTrigger = 15;

            switch (taskname)
            {
                case Settings.ChangeWallpaperTaskName:
                    minutesForTrigger = (uint)s.IntervalForChangeWallPaper;
                    break;
                case Settings.SearchPicturesTaskName:
                    minutesForTrigger = (uint)s.IntervalForSearchPictures;
                    break;
                default:
                    minutesForTrigger = 15;
                    break;
            }
            return new TimeTrigger(minutesForTrigger, false);
        }
        #endregion

        #region logEntries
        public static IList<LogEntry> GetAllLogs()
        {
            IList<LogEntry> logs;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                logs = (from p in db.Table<LogEntry>()
                        select p).OrderByDescending(d => d.Id).ToList();
            }

            return logs;
        }
        public static IList<LogEntry> GetLatestXLogs(int x)
        {
            IList<LogEntry> logs;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                logs = (from p in db.Table<LogEntry>()
                        select p).OrderByDescending(d => d.Id).Take(x).ToList();
            }

            return logs;
        }

        public static void DeleteAllLogEntries()
        {
            try
            {
                // Create a new connection
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    // SQL Syntax:
                    db.Execute("DELETE FROM LogEntry");
                }
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

        public static void SaveLogEntry(LogType ltype, string logDescription)
        {
            // 
            // CHeck when Must Save Log Entry 
            //
            Setup n = GetSetup();

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
                    lentry.LogEntryDate = DateTime.Now.ToString();
                    // Create a new connection
                    using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                    {
                        // New
                        db.Insert(lentry);
                        System.Diagnostics.Debug.WriteLine("Log: " + lentry.LogType + " " + logDescription);
                    }
                }
                catch (Exception ex)
                {
                    SaveLogEntry(LogType.Error, "Exception in SaveLogEntry() " + ex.Message);
                }
            }
        }
        #endregion

        #region Message
        public static void SaveMessage(string message)
        {
            try
            {
                // Create a new connection
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    // New
                    db.Insert(new Message()
                    {
                        Content = message
                    });
                    System.Diagnostics.Debug.WriteLine("Message Inserted");
                }
            }
            catch (Exception ex)
            {
                //SaveLogEntry(LogType.Error, "Exception in SaveLogEntry() " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Error Message Inserted");
            }
        }

        public static IList<Message> GetMessages()
        {
            IList<Message> messages;
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                messages = (from p in db.Table<Message>()
                            select p).OrderByDescending(d => d.Id).ToList();
            }

            return messages;
        }
        #endregion

        #region pictures
        public static void DeletePicture(FavoritePic pic)
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                //db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM FavoritePic WHERE Id = ?", pic.Id);
            }
        }

        public static void DeleteAllPictures()
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                //db.TraceListener = new DebugTraceListener();

                // SQL Syntax:
                db.Execute("DELETE FROM FavoritePic");
            }
        }

        public static IList<FavoritePic> GetAllPictures()
        {
            IList<FavoritePic> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                //db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<FavoritePic>()
                          select p).ToList();
            }

            return models;
        }

        public static async Task<FavoritePic> GetRandomPicture()
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                var m = (from p in db.Table<FavoritePic>()
                         select p).Where(v => v.Viewed == false && v.DownloadedFromOneDrive==true).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (m == null)
                    m = (from p in db.Table<FavoritePic>() select p).FirstOrDefault();

                return m;
            }
        }

        public static void CheckForViewedPictures()
        {
            try
            {
                // Create a new connection
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    var viewedPics = (from p in db.Table<FavoritePic>()
                                      select p).Where(v => v.Viewed == false).ToList();
                    if (viewedPics.Count == 0)
                    {
                        var upPics = (from p in db.Table<FavoritePic>()
                                      select p).Where(v => v.Viewed == true).ToList();

                        foreach (FavoritePic p in upPics)
                        {
                            p.Viewed = false;
                            SavePicture(p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLogEntry(LogType.Error, "Exception in CheckForViewed Pictures " + ex.Message);
            }

        }

        public static FavoritePic GetPictureById(int Id)
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                // Activate Tracing
                //db.TraceListener = new DebugTraceListener();
                FavoritePic m = (from p in db.Table<FavoritePic>()
                                 where p.Id == Id
                                 select p).FirstOrDefault();
                return m;
            }
        }

        /// <summary>
        /// Reset All Fields IsCurrentWallpaper in Table FavoritePic
        /// </summary>
        public static void ResetIsCurrentWallpaper()
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                db.Query<FavoritePic>("UPDATE FavoritePic SET IsCurrentWallPaper = 0");
            }
        }

        public static FavoritePic GetCurrentBGPic()
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                FavoritePic m = (from p in db.Table<FavoritePic>()
                                 where p.IsCurrentWallPaper == true
                                 select p).FirstOrDefault();
                return m;
            }
        }

        public static void SavePicture(FavoritePic pic)
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                if (pic.Id == 0)
                {
                    //TODO: First Reset all Current BG Pics Status 
                    //var allPictures = (from p in db.Table<FavoritePic>()
                    //          select p).ToList();
                    ////https://code.msdn.microsoft.com/windowsapps/Sqlite-Sample-for-Windows-ad3af7ae
                    db.Query<FavoritePic>("UPDATE FavoritePic SET IsCurrentWallPaper = 0");

                    // New
                    db.Insert(pic);
                }
                else
                {
                    // Update
                    db.Update(pic);
                }
            }
        }
        #endregion

        #region BGTasks
        public static void DeleteAllTaskStatus()
        {
            try
            {
                // Create a new connection
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    // SQL Syntax:
                    db.Execute("DELETE FROM TaskStatus");
                }
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

        public static BGTask GetTaskStatusByTaskName(string tName)
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                BGTask t = (from p in db.Table<BGTask>()
                                where p.TaskName == tName
                                select p).FirstOrDefault();
                return t;
            }
        }

        public static string GetTimeFromLastRun(string name)
        {
            var ts = GetTaskStatusByTaskName(name);
            return ts.LastTimeRun;
        }

        public static void UpdateTaskStatus(BGTask ts)
        {
            try
            {
                // Create a new connection
                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
                {
                    // New
                    if (GetTaskStatusByTaskName(ts.TaskName) != null)
                    {
                        db.Update(ts);
                        SaveLogEntry(LogType.Info, "DB Update TaskStatus " + ts.TaskName + " Current Status: " + ts.CurrentRegisteredStatus);
                    }
                    else
                    {
                        db.Insert(ts);
                        SaveLogEntry(LogType.Info, "DB Save TaskStatus " + ts.TaskName + " Current Status: " + ts.CurrentRegisteredStatus);
                    }


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

        public static BGTask SetCurrentRegistrationStatus(string taskname, bool registered)
        {
            BGTask ts = GetTaskStatusByTaskName(taskname);
            ts.CurrentRegisteredStatus = registered;
            return ts;
        }

        public static IList<BGTask> GetAllTaskStatus()
        {
            IList<BGTask> mlist;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {

                mlist = (from p in db.Table<BGTask>()
                         select p).ToList();
            }

            return mlist;
        }


        #endregion

        #region MS Graph
        public static FavoritePic GetRandomInfoItemResponse()
        {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath))
            {
                var m = (from p in db.Table<FavoritePic>()
                         select p).Where(v => v.Viewed == false && v.DownloadedFromOneDrive ==true).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (m == null)
                    m = (from p in db.Table<FavoritePic>() select p).FirstOrDefault();

                return m;
            }
            
        }

        public static async Task LoadImagesFromOneDriveInDBTable()
        {
            Exception error = null;
            ItemInfoResponse folder = null;
            ItemInfoResponse rootfolder = null;
            IList<ItemInfoResponse> children = null;

            //// Initialize Graph client
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);

            try
            {
                rootfolder = await graphService.GetAppRoot();
                //folder = await graphService.GetPhotosAndImagesFromFolder("/Bilder/Karneval2019");
                folder = await graphService.GetPhotosAndImagesFromFolder("/Bilder/WindowsIotApp" +
                    "");
                children = await graphService.PopulateChildren(folder);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                SaveLogEntry(LogType.Error, error.Message);
                return;
            }

            //https://gunnarpeipman.com/csharp/foreach/
            foreach (ItemInfoResponse iir in children.ToList())
            {
                if (iir.Image != null)
                {
                    System.Diagnostics.Debug.WriteLine("PhotoName: " + iir.Name + "Id: " + iir.Id);
                    //iri = iir;
                }
                else
                {
                    children.Remove(iir);
                }
            }
            foreach (var iri in children)
            {
                var fp = new FavoritePic();

                fp.DownloadedFromOneDrive = true;
                fp.Viewed = false;
                fp.DownloadUrl = iri.DownloadUrl;
                fp.Name = iri.Name;
                fp.OneDriveId = iri.Id;
                SavePicture(fp);
            }

        }
        #endregion

    }
}
