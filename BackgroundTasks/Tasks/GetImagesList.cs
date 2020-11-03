using AppSettings;
using MSGraph;
using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace RWPBGTasks
{
    public sealed class GetImageListFromOneDrive : IBackgroundTask
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
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting..." + " at " + DateTime.Now);

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

                await LoadImageListFromOneDrive();
            }
            catch (Exception ex)
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception in Run() Task GetImageListFromOneDrive " + ex.Message);
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
        private async Task LoadImageListFromOneDrive()
        {
            await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Entry in LoadImageListFromOneDrive()");

            if ((_cancelRequested == false) && (_progress < 100))
            {
            }
            try
            {
                Exception error = null;
                ItemInfoResponse folder = null;
                IList<ItemInfoResponse> children = null;

                //// Initialize Graph client
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);

                try
                {
                    var s = await DAL.AppDataBase.GetSetup();
                    folder = await graphService.GetPhotosAndImagesFromFolder(s.OneDrivePictureFolder);
                    children = await graphService.PopulateChildren(folder);

                    if (children != null)
                    {

                        try
                        {

                            //https://gunnarpeipman.com/csharp/foreach/
                            ///TODO: Null Exception here when Children is null 
                            int xyz = 0;
                            foreach (ItemInfoResponse iir in children.ToList())
                            {
                                if (iir.Image != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine("PhotoName : " +xyz+ " - "  + iir.Name + "Id: " + iir.Id);
                                    xyz += 1;
                                    //iri = iir;
                                }
                                else
                                {
                                    children.Remove(iir);
                                }
                            }
                            xyz =0;
                            int totalFiles = children.Count;
                            int filesProcessed = 0;
//                            await HelloWindowsIotDataBase.DeleteAllPictures();
                            foreach (var iri in children)
                            {
                                if (iri.Image != null)
                                {

                                    filesProcessed++;
                                    _progress = (uint)((double)filesProcessed / totalFiles * 100);
                                    _taskInstance.Progress = _progress; ///=> !!!!!!!!
                                    var dbPic = DAL.AppDataBase.GetPictureByOneDriveId(iri.Id);
                                    if (dbPic == null)
                                    {
                                        var fp = new FavoritePic();
                                        fp.DownloadedFromOneDrive = true;
                                        fp.Viewed = false;
                                        fp.Name = iri.Name;
                                        fp.DownloadUrl = iri.DownloadUrl;
                                        fp.Name = iri.Name;
                                        fp.Description = iri.Description;
                                        fp.OneDriveId = iri.Id;
                                        fp.Status = "UpToDate";
                                        System.Diagnostics.Debug.WriteLine("New Pic in DB : " + xyz + " - " + iri.Name + "Id: " + iri.Id);
                                        await DAL.AppDataBase.SavePicture(fp);
                                    }
                                    else
                                    {
                                        var fp = dbPic;
                                        fp.Status = "UpToDate";
                                        fp.Description = iri.Description;
                                        System.Diagnostics.Debug.WriteLine("Pic Update in DB PhotoName : " + xyz + " - " + iri.Name + "Id: " + iri.Id + "Desc: " + iri.Description);
                                        await DAL.AppDataBase.SavePicture(fp);
                                    }
                                    xyz += 1;
                                    
                                }
                                
                            }

                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Exception in LoadImageListFromOneDrive: " + ex.Message );
                            await DAL.AppDataBase.SaveLogEntry(LogType.Error, ex.Message);
                        }
                        finally
                        {
                            DAL.AppDataBase.DelIndefinablePics();
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
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
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " + DateTime.Now + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);
            }
        }
        #endregion
    }
}