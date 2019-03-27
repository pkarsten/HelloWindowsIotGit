using AppSettings;
using MSGraph;
using MSGraph.Response;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace RWPBGTasks
{
    public sealed class StreamImage : IBackgroundTask
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
                Dal.SaveLogEntry(LogType.Info, "Background " + taskInstance.Task.Name + " Starting..." + " at " + DateTime.Now);

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
                    Dal.SaveLogEntry(LogType.Info, "Background Cost " + BackgroundWorkCost.CurrentBackgroundWorkCost + "in " + taskInstance.Task.Name);
                }
                else
                {
                    // Do All Work
                    if (_cancelRequested)
                    {
                    }
                }

                await LoadImageForDashBoard();

            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception in Run() Task StreamImageAsync " + ex.Message);
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
        private async Task LoadImageForDashBoard()
        {
            if ((_cancelRequested == false))
            {
                try
                {
                    // Get Random ItemInfoResponse from Table 
                    var item = Dal.GetRandomInfoItemResponse();

                    BitmapImage bitmapimage = new BitmapImage();

                    // Only load a detail view image for image items. Initialize the bitmap from the image content stream.
                    Exception error = null;
                    ItemInfoResponse foundFile = null;
                    Stream contentStream = null;

                    //ShowBusy(true);
                    //// Initialize Graph client
                    var accessToken = await GraphService.GetTokenForUserAsync();
                    var graphService = new GraphService(accessToken);
                    try
                    {
                        foundFile = await graphService.GetItem(item.OneDriveId);

                        if (foundFile == null)
                        {
                            Dal.SaveLogEntry(LogType.Error, $"Image Not found Id: {item.OneDriveId}");
                            return;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + "Id: " + item.OneDriveId + item.DownloadUrl);

                        }

                        // Get the file's content
                        contentStream = await graphService.RefreshAndDownloadContent(foundFile, false);

                        if (contentStream == null)
                        {
                            Dal.SaveLogEntry(LogType.Error, $"Content not found: {foundFile.Name}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }

                    if (error != null)
                    {
                        Dal.SaveLogEntry(LogType.Error, error.Message);
                        return;
                    }

                    // Save the retrieved stream 
                    var memoryStream = contentStream as MemoryStream;

                    if (memoryStream != null)
                    {
                        //if (item.Image == null)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("item.Image == null");
                        //}
                        System.Diagnostics.Debug.WriteLine("memoryStream != null");
                        //await item.Image.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        System.Diagnostics.Debug.WriteLine("awaited memory stream != null");

                    }
                    else
                    {
                        using (memoryStream = new MemoryStream())
                        {
                            await contentStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            System.Diagnostics.Debug.WriteLine("using (memoryStream = new MemoryStream()");
                            await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("must set bgimage");

                    //bitmapimage = new BitmapImage(new Uri(item.DownloadUrl)); -> Works too
                    Settings.DashBoardImage = bitmapimage;
                    _progress = 100;
                }
                catch (Exception ex)
                {
                    await Dal.SaveLogEntry(LogType.Error, "Exception  in StreamImageAsync() " + ex.Message);
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
                    await Dal.UpdateTaskStatus(ts);
                    await Dal.SaveLogEntry(LogType.Info, "Background " + _taskInstance.Task.Name + " is Finished at " + DateTime.Now + "Additional Status is " + _taskInstance.Task.Name + settings.Values[key]);
                }

            }
        }
        #endregion
    }
}