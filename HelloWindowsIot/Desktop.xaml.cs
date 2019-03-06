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
        }

        private async Task GetImageForDesktop()
        {
            
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

        /// <summary>
        /// Loads the detail view for the specified item.
        /// </summary>
        /// <param name="item">The item to load.</param>
        /// <returns>The task to await.</returns>
        private async Task LoadImage(ImageResponseInfo item)
        {
            // Only load a detail view image for image items. Initialize the bitmap from the image content stream.
            if (item.Bitmap == null)
            {
                item.Bitmap = new BitmapImage();

                //// Initialize Graph client
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);

                var t = graphService.GetOneDriveItemAsync(item.Id);

                using (var responseStream = await graphService.GetOneDriveItemAsync(item.Id); //await client.Drive.Items[item.Id].Content.Request().GetAsync())
                {
                    var memoryStream = responseStream as MemoryStream;

                    if (memoryStream != null)
                    {
                        await item.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    }
                    else
                    {
                        using (memoryStream = new MemoryStream())
                        {
                            await responseStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;

                            await item.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        }
                    }
                }
            }
        }
    }
}
