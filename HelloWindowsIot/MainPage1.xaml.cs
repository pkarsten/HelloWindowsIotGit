using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MSGraph.Response;
using MSGraph.Request;
using Windows.UI.Popups;
using MSGraph.Helpers;
using MSGraph;

namespace HelloWindowsIot
{
    public sealed partial class MainPage1 : Page
    {
        //Set the API Endpoint to Graph 'me' endpoint // Hallo // Hallo2 sdfg dsg asdf 
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";
        //5
        // http://thewindowsupdate.com/2019/01/29/exploring-microsoft-the-graph-sdk-on-net/

        private const string RequestRootFolder = "/drive/root";
        private const string RequestSpecialFolder = "/drive/special/Photos";

        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read", "Files.Read", "Calendars.Read" };

        public MainPage1()
        {
            this.InitializeComponent();
        }

        private async void GetAppRootFolder(object sender, RoutedEventArgs e)
        {

            Exception error = null;
            ItemInfoResponse folder = null;
            IList<ItemInfoResponse> children = null;

            //// Initialize Graph client
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);
            ShowBusy(true);

            try
            {
                folder = await graphService.GetSpecialFolder(SpecialFolder.Photos);
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

            DisplayHelper.ShowContent(
                "SHOW ROOT FOLDER ++++++++++++++++++++++",
                folder,
                children,
                async message =>
                {
                    var dialog = new MessageDialog(message);
                    await dialog.ShowAsync();
                });

            ShowBusy(false);
        }

        private async void GetPhotosAndImagesFromFolder(object sender, RoutedEventArgs e)
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
                folder = await graphService.GetPhotosAndImagesFromFolder("/Bilder/Karneval2019");
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

            foreach(ItemInfoResponse iir in children)
            {
                if (iir.Image != null)
                {
                    System.Diagnostics.Debug.WriteLine("PhotoName: " + iir.Name + "Id: " + iir.Id);
                }
            }

            DisplayHelper.ShowContent(
               "SHOW Item Properties ++++++++++++++++++++++",
               folder,
               children,
               async message =>
               {
                   var dialog = new MessageDialog(message);
                   await dialog.ShowAsync();
               });

            ShowBusy(false);
        }

        /// <summary>
        /// Call AcquireTokenAsync - to acquire a token requiring user to sign-in
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = await GraphService.GetAuthResult();
            ResultText.Text = string.Empty;
            TokenInfoText.Text = string.Empty;

            if (authResult != null)
            {
                ResultText.Text = await GraphService.GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                DisplayBasicTokenInfo(authResult);
                this.SignOutButton.Visibility = Visibility.Visible;
            }
        }

        private async void GetFirstCalenderView_CLick(object sender, RoutedEventArgs e)
        {
            //// Initialize Graph client
            var accessToken = await GraphService.GetTokenForUserAsync();
            var graphService = new GraphService(accessToken);
            //CalendarText.Text = await graphService.GetCalendarViewTest();
            IList<CalendarEventItem> myevents = await graphService.GetCalendarEvents();
            foreach (CalendarEventItem ce in myevents)
            {
                System.Diagnostics.Debug.WriteLine("Date : " + ce.StartDateTime.dateTime + " Subject: " + ce.Subject);
            }
        }


        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            //IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            //IAccount firstAccount = accounts.FirstOrDefault();
            //try
            //{
            //    await App.PublicClientApp.RemoveAsync(firstAccount);
            //    this.ResultText.Text = "User has signed-out";
            //    this.CallGraphButton.Visibility = Visibility.Visible;
            //    this.SignOutButton.Visibility = Visibility.Collapsed;
            //}
            //catch (MsalException ex)
            //{
            //    ResultText.Text = $"Error signing-out user: {ex.Message}";
            //}

            if (await GraphService.SignOut() == true)
            {
                this.ResultText.Text = "User has signed-out";
                this.CallGraphButton.Visibility = Visibility.Visible;
                this.SignOutButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResultText.Text = $"Error signing-out user: ";
            }

        }

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
                TokenInfoText.Text += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
            }
        }

        private void ShowBusy(bool isBusy)
        {
            Progress.IsActive = isBusy;
            PleaseWaitCache.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GoToDesktopClick(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(Desktop));
        }
    }
}
