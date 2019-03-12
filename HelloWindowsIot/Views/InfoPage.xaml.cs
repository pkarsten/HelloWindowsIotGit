using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using RWPBGTasks;
using Windows.ApplicationModel.Contacts;
using Windows.System;
using Windows.Services.Store;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Resources.Core;

namespace HelloWindowsIot
{

    public sealed partial class InfoPage : Page
    {
        private MainPage rootPage = MainPage.Current;

        public InfoPage()
        {
            this.InitializeComponent();
            txtInfoPageVersion.Text = "Version: " + AppInfos.ApplicationVersion;
            txtAppName.Text = AppInfos.ApplicationName;

        }
        #region Eventhandler
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AppcFuncs.InitializeLicence();
            AppcFuncs.SContext.OfflineLicensesChanged += OfflineLicensesChanged;

            StoreProductResult result = await AppcFuncs.SContext.GetStoreProductForCurrentAppAsync();
            if (result.ExtendedError == null)
            {
                PurchasePrice.Text = result.Product.Price.FormattedPrice;
            }

            await GetLicenseState();
            ShowTrialPeriodInformation();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            AppcFuncs.SContext.OfflineLicensesChanged -= OfflineLicensesChanged;
        }

        private void OfflineLicensesChanged(StoreContext sender, object args)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await GetLicenseState();
            });
        }



        private async void SupportBtn_Click(object sender, RoutedEventArgs e)
        {
            await CreateEMail();
        }

        private async void RatingBtn_Click(object sender, RoutedEventArgs e)
        {
            await OpenStore();

        }
        #endregion

        #region OpenSTore Rating
        public async Task OpenStore()
        {
            var uri = new Uri("ms-windows-store://review/?ProductId=" + Settings.ProductIdinStore);
            await Launcher.LaunchUriAsync(uri);
        }
        #endregion

        #region Email
        private async Task CreateEMail()
        {
            Contact c = new Contact();
            c.FirstName = Settings.SupporterFirstName;

            ContactEmail em = new ContactEmail();
            em.Address = Settings.SupportEmail;
            c.Emails.Add(em);

            string subject = AppcFuncs.GetLanguage("supportRequest")  + AppInfos.ApplicationName;

            string body = "";
            body += AppcFuncs.GetLanguage("hello")+ " Peter, \n\n"; 
            body += "\n\n";
            body += AppInfos.ApplicationName + "version: " + AppInfos.ApplicationVersion + Environment.NewLine;
            body += "System Family: " + AppInfos.SystemFamily + Environment.NewLine;
            body += "System Version: " + AppInfos.SystemVersion + Environment.NewLine;
            body += "System Architecture: " + AppInfos.SystemArchitecture + Environment.NewLine;
            body += "Device Manufacturer: " + AppInfos.DeviceManufacturer + Environment.NewLine;
            body += "Device Model: " + AppInfos.DeviceModel + Environment.NewLine;
            body += Environment.NewLine;

            body += "Last 100 Logs :" + Environment.NewLine;
            body += "---------------" + Environment.NewLine;
            foreach (LogEntry le in Dal.GetLatestXLogs(100))
            {
                body += le.LogEntryDate + " " + le.LogType + " " + le.Description + Environment.NewLine;
            }


            // For Attach File to E-Mail (Only works with App STore Mail Clients) 
            // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/5a539ff6-ce81-4f05-83a0-8bf430d827f8/uwpwindows-10-c-universal-windows-app-email-with-attachment?forum=wpdevelop&prof=required
            //StorageFile logfile = await CreateLogFile();

            await ComposeEmail(c, body, subject, null);
        }

        private async Task ComposeEmail(Windows.ApplicationModel.Contacts.Contact recipient, string messageBody, string subject, StorageFile attachmentFile)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = subject;
            emailMessage.Body = messageBody;


            if (attachmentFile != null)
            {
                var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);

                var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                    attachmentFile.Name,
                    stream);

                emailMessage.Attachments.Add(attachment);
            }

            var email = recipient.Emails.FirstOrDefault<Windows.ApplicationModel.Contacts.ContactEmail>();
            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                emailMessage.To.Add(emailRecipient);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }

        private async Task<StorageFile> CreateLogFile()
        {
            // https://microsoft-programmierer.de/Details?d=1352&a=9&f=191&l=0&t=new-20.12.2015-17:24:40
            // and 
            // https://docs.microsoft.com/de-de/windows/uwp/files/quickstart-reading-and-writing-files
            // or 
            // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/5a539ff6-ce81-4f05-83a0-8bf430d827f8/uwpwindows-10-c-universal-windows-app-email-with-attachment?forum=wpdevelop
            try
            {
                string sfile = "log.txt";
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile textFile = await storageFolder.CreateFileAsync(sfile, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                //Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(sfile);

                // First Step
                var stream = await textFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                //Second Step
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        //dataWriter.WriteString("DataWriter has methods to write to various types, such as DataTimeOffset.");
                        foreach (LogEntry le in Dal.GetAllLogs())
                        {
                            dataWriter.WriteString(le.LogEntryDate + " " + le.LogType + " " + le.Description + Environment.NewLine);
                        }
                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                    
                }
                stream.Dispose(); // Or use the stream variable (see previous code snippet) with a using statement as well.

                return await storageFolder.GetFileAsync(sfile);
            }
            catch (Exception ex)

            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in createLogFile " +ex.Message);
                return null;
            }
        }
        #endregion

        #region Store Trial Buy 

        /// <summary>
        /// Get Licence State
        /// </summary>
        /// <returns></returns>
        private async Task GetLicenseState()
        {
            var lf = await AppcFuncs.LizenzInfos();
            LicenseMode.Text = lf.Message;
        }
        /// <summary>
        /// Invoked when the user asks to see trial period information.
        /// </summary>
        private async void ShowTrialPeriodInformation()
        {
            var licenceInfo = await AppcFuncs.LizenzInfos();

            if (licenceInfo.IsActive)
            {
                if (licenceInfo.IsTrial)
                {
                    int remainingTrialTime = (licenceInfo.ExpirationDate - DateTime.Now).Days;
                    txtTrialPeriodInformation.Text = String.Format(AppSettings.AppResourceMap.GetValue("txtTrialPeriodInformation1", ResourceContext.GetForCurrentView()).ValueAsString, remainingTrialTime);
                }
                else
                {
                    txtTrialPeriodInformation.Text = AppcFuncs.GetLanguage("txtTrialPeriodInformation2");

                }
            }
            else
            {
                txtTrialPeriodInformation.Text = AppcFuncs.GetLanguage("txtTrialPeriodInformation3");
            }
        }

        /// <summary>
        /// Invoked when the user asks purchase the app.
        /// </summary>
        private async void PurchaseFullLicense()
        {
            // Get app store product details
            StoreProductResult productResult = await AppcFuncs.SContext.GetStoreProductForCurrentAppAsync();
            if (productResult.ExtendedError != null)
            {
                // The user may be offline or there might be some other server failure
                RefreshStatus($"ExtendedError: {productResult.ExtendedError.Message}", NotifyType.ErrorMessage);
                return;
            }

            RefreshStatus(AppcFuncs.GetLanguage("fullLicence1"), NotifyType.StatusMessage);
            StoreAppLicense license = await AppcFuncs.SContext.GetAppLicenseAsync();
            if (license.IsTrial)
            {
                StorePurchaseResult result = await productResult.Product.RequestPurchaseAsync();
                if (result.ExtendedError != null)
                {
                    RefreshStatus($""+AppcFuncs.GetLanguage("fullLicence2") +": ExtendedError: {result.ExtendedError.Message}", NotifyType.ErrorMessage);
                    return;
                }

                switch (result.Status)
                {
                    case StorePurchaseStatus.AlreadyPurchased:
                        RefreshStatus($""+AppcFuncs.GetLanguage("AlreadyPurchased") +"", NotifyType.ErrorMessage);
                        break;

                    case StorePurchaseStatus.Succeeded:
                        // License will refresh automatically using the StoreContext.OfflineLicensesChanged event
                        break;

                    case StorePurchaseStatus.NotPurchased:
                        RefreshStatus($"" + AppcFuncs.GetLanguage("NotPurchased") + "", NotifyType.ErrorMessage); 
                        break;
                    case StorePurchaseStatus.NetworkError:
                        RefreshStatus($"" + AppcFuncs.GetLanguage("NetworkError") + "", NotifyType.ErrorMessage);
                        break;

                    case StorePurchaseStatus.ServerError:
                        RefreshStatus($"" + AppcFuncs.GetLanguage("ServerError") + "", NotifyType.ErrorMessage);
                        break;

                    default:
                        RefreshStatus($"" + AppcFuncs.GetLanguage("UnknownError") + "", NotifyType.ErrorMessage);
                        break;
                }
            }
            else
            {
                RefreshStatus($"" + AppcFuncs.GetLanguage("AlreadyPurchased") + "", NotifyType.ErrorMessage);
            }
        }
        #endregion

        /// <summary>
        /// refresh Status Block  
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        private async void RefreshStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
