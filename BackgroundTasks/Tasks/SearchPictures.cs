using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;

namespace RWPBGTasks
{
    //
    // A background task always implements the IBackgroundTask interface.
    //
    public sealed class SearchPictures : IBackgroundTask
    {
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        uint _progress = 0;
        IBackgroundTaskInstance _taskInstance = null;

        //
        // The Run method is the entry point of a background task.
        //
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                Dal.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting...");
                
                //
                // Create Database if not exist
                //
                Dal.CreateDatabase();


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
                    //Do less things if Backgroundcost is high
                    Dal.SaveLogEntry(LogType.Info, "Background Cost" + BackgroundWorkCost.CurrentBackgroundWorkCost);
                }
                else
                {
                    // Do All Work
                }
                await SearchPicturesAsync();
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception in Run() Task SearchPictures " + ex.Message);
            }

            finally
            {
                if (_deferral != null)
                {
                    // Inform the system that the task is finished.
                    _deferral.Complete();
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

            Dal.SaveLogEntry(LogType.Error, "Background " + sender.Task.Name + " Cancel Requested...");
        }

        //
        // The background task activity.
        //
        private async Task SearchPicturesAsync()
        {
            Dal.SaveLogEntry(LogType.Info, "Entry in SearchPicturesAsync()");

            if ((_cancelRequested == false) && (_progress < 100))
            {
                try
                {
                    Dal.DeleteAllPictures();

                    // Search Images in Picture Library 
                    StorageFolder picturesFolder = KnownFolders.PicturesLibrary;

                    StorageLibrary myPicturesLib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                    IObservableVector<Windows.Storage.StorageFolder> myPictureLibFolders = myPicturesLib.Folders;

                    PicFilter currPicFilter = Dal.GetPicFilter();

                    // Group Pictures by Rating Property 
                    Enum.TryParse(currPicFilter.CommonFolderQuery, out CommonFolderQuery queryToApply);
                    Dal.SaveLogEntry(LogType.Info, "CommonFolderQuery is " + queryToApply);
                    StorageFolderQueryResult queryResult = picturesFolder.CreateFolderQuery(queryToApply);

                    // Query found Folder (groups) e.g. 5 Star, 4 Star, 3 Stars,... No in a Group 
                    IReadOnlyList<StorageFolder> resultList = await queryResult.GetFoldersAsync();

                    IReadOnlyList<StorageFile> _favoriteList;
                    if (resultList != null)
                    {

                        if (currPicFilter.VirtualFolder == "")
                        {
                            //Get First Folder (best rated) 
                            _favoriteList = await resultList.First().GetFilesAsync();
                        }
                        else
                        {
                            // get last Known Virtual Folder
                            _favoriteList = await resultList.Where(f => f.DisplayName == currPicFilter.VirtualFolder).First().GetFilesAsync();

                            // if no pics in last known folder, get default folder (the first)
                            if (_favoriteList.Count == 0)
                            {
                                _favoriteList = await resultList.First().GetFilesAsync();
                            }
                        }

                        int totalFiles = _favoriteList.Count;
                        int filesProcessed = 0;
                        foreach (StorageFile favStorageFile in _favoriteList)
                        {

                            foreach (var fold in myPictureLibFolders)
                            {
                                if (StorageFileExtensions.LibFolderContainsTheImage(fold.Path, StorageFileExtensions.GetDirectory(favStorageFile)) == true)
                                {
                                    filesProcessed++;
                                    _progress = (uint)((double)filesProcessed / totalFiles * 100);
                                    _taskInstance.Progress = _progress;
                                    FavoritePic fp = new FavoritePic();
                                    fp.Stars = 5;
                                    fp.RelativePath = StorageFileExtensions.GetAbsPath(favStorageFile, fold.Path);
                                    fp.Name = favStorageFile.Name;
                                    fp.LibraryPath = fold.Path;
                                    Dal.SavePicture(fp);
                                    
                                }
                            }
                        }

                        //Dal.SaveLogEntry(LogType.Info, String.Format("Searched for Pictures, total Files Found: {0}. Files Processed: {1}", totalFiles, filesProcessed));
                        Dal.SaveLogEntry(LogType.AppInfo, String.Format("Searched for Pictures, total Files Found: {0}.", totalFiles));
                    }
                    Dal.DeleteAllPictures();
                    await Dal.LoadImagesFromOneDriveInDBTable("/Bilder/WindowsIotApp");//TODO Add variable here 

                    _progress = 100;
                }
                catch (Exception ex)
                {
                    Dal.SaveLogEntry(LogType.Error, "Exception  in SearchPicturesAsync() " + ex.Message);
                }
                finally
                {
                    var settings = ApplicationData.Current.LocalSettings;
                    var key = _taskInstance.Task.Name;

                    //
                    // Write to LocalSettings to indicate that this background task ran.
                    //
                    settings.Values[key] = (_progress < 100) ? "Canceled with reason: " + _cancelReason.ToString() : "Completed";
                    UwpSqliteDal.BGTask ts = Dal.GetTaskStatusByTaskName(_taskInstance.Task.Name);
                    ts.LastTimeRun = DateTime.Now.ToString();
                    ts.AdditionalStatus = settings.Values[key].ToString();
                    Dal.UpdateTaskStatus(ts);
                    Dal.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " +DateTime.Now + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);
                }
            }
        }
    }
}
