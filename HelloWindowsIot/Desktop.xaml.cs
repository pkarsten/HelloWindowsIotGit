using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MSGraph.Response;
using Windows.UI.Xaml.Media.Imaging;
using MSGraph;
using Windows.UI.Popups;
using MSGraph.Helpers;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace HelloWindowsIot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Desktop : Page
    {
        public Desktop()
        {
            this.InitializeComponent();
            LoadImagesFromOneDrive();
            GetCalendarView();

            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();


        }

        DispatcherTimer Timer = new DispatcherTimer();

        private void Timer_Tick(object sender, object e)
        {
            MyTime.Text = DateTime.Now.ToString("h:mm:ss tt");
        }

        private async void GetCalendarView()
        {
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);
            //CalendarText.Text = await graphService.GetCalendarViewTest();
            string s = "";
            IList<CalendarEventItem> myevents = await graphService.GetCalendarEvents();
            foreach (CalendarEventItem ce in myevents)
            {
                string d = String.Format("{0:dd.MM/yyyy}", ce.StartDateTime.dateTime);  // "03/09/2008"
                s = s + d + ": " + ce.Subject + " \n";
                System.Diagnostics.Debug.WriteLine(s);
            }
            MyEvents.Text = s;
        }


        private async void LoadImagesFromOneDrive()
        {
                Exception error = null;
                ItemInfoResponse folder = null;
                ItemInfoResponse rootfolder = null;
                IList<ItemInfoResponse> children = null;

                //// Initialize Graph client
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);
                ShowBusy(true);

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
                    var dialog = new MessageDialog(error.Message, "Error!");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                    return;
                }

            ItemInfoResponse iri = new ItemInfoResponse();
            // iri = children.First();

            foreach (ItemInfoResponse iir in children)
                {
                    if (iir.Image != null)
                    {
                        System.Diagnostics.Debug.WriteLine("PhotoName: " + iir.Name + "Id: " + iir.Id);
                    iri = iir;
                    } else
                    {
                        children.Remove(iir);
                    }
                }

            Random _random1 = new Random(DateTime.Now.Millisecond);
            var iri1 = children[_random1.Next(0, children.Count)];
            await LoadImageForDesktop(iri1);
            ShowBusy(false);

            TimeSpan period = TimeSpan.FromSeconds(20);
            // display neew images every five seconds
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(
                async (source) =>
                {
                    Random _random = new Random(DateTime.Now.Millisecond);
                    iri = children[_random.Next(0, children.Count)];
                    if (iri != null)
                    {
                        // we have to update UI in UI thread only
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async() =>
                        {
                            //ShowBusy(true);
                            // create and load bitmap
                            await LoadImageForDesktop(iri);
                        }
                        );
                    }
                }, period);

            //DisplayHelper.ShowContent(
            //    "SHOW FOLDER ++++++++++++++++++++++",
            //    folder,
            //    children,
            //    async message =>
            //    {
            //        var dialog = new MessageDialog(message);
            //        await dialog.ShowAsync();
            //    });

            ShowBusy(false);
        }
        
        /// <summary>
        /// Loads the detail view for the specified item.
        /// </summary>
        /// <param name="item">The item to load.</param>
        /// <returns>The task to await.</returns>
        private async Task LoadImageForDesktop(ItemInfoResponse item)
        {
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
                foundFile = await graphService.GetItem(item.Id);

                if (foundFile == null)
                {
                    var dialog = new MessageDialog($"Image Not found Id: {item.Id}");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                    return;
                } else
                {
                    System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + "Id: " + item.Id + item.DownloadUrl);

                }

                // Get the file's content
                contentStream = await graphService.RefreshAndDownloadContent(foundFile, false);

                if (contentStream == null)
                {
                    var dialog = new MessageDialog($"Content not found: {foundFile.Name}");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                    return;
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

                // Save the retrieved stream 
                var memoryStream = contentStream as MemoryStream;

                if (memoryStream != null)
                {
                    if (item.Image == null)
                    {
                        System.Diagnostics.Debug.WriteLine("item.Image == null");
                    }
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

            BGImage.Source = bitmapimage;

        }

        /// <summary>
        /// Handle Fullscreen Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetFullScreenCLick(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }
        //Handle Progress loading Ring 
        private void ShowBusy(bool isBusy)
        {
            Progress.IsActive = isBusy;
            PleaseWaitCache.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
