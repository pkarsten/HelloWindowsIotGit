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
using UwpSqliteDal;
using AppSettings;

namespace HelloWindowsIot
{

    public sealed partial class InfoPage : Page
    {
        private MainPage rootPage = MainPage.Current;
        public InfoViewModel ViewModel { get; set; }

        public InfoPage()
        {
            this.InitializeComponent();
            this.ViewModel = new InfoViewModel();
            //txtInfoPageVersion.Text = "Version: " + AppInfos.ApplicationVersion;
            //txtAppName.Text = AppInfos.ApplicationName;

        }

        #region Navigate
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await HelloWindowsIotDataBase.SaveLogEntry(LogType.Info, "Navigated To InfoPage");
            await ViewModel.LoadData();
        }
        #endregion
        //#region Eventhandler
        //private async void SupportBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    await CreateEMail();
        //}

        //private async void RatingBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    await OpenStore();

        //}
        //#endregion

        #region OpenSTore Rating
        //public async Task OpenStore()
        //{
        //    var uri = new Uri("ms-windows-store://review/?ProductId=" + Settings.ProductIdinStore);
        //    await Launcher.LaunchUriAsync(uri);
        //}
        #endregion

        #region Email
        //private async Task CreateEMail()
        //{
        //    Contact c = new Contact();
        //    c.FirstName = Settings.SupporterFirstName;

        //    ContactEmail em = new ContactEmail();
        //    em.Address = Settings.SupportEmail;
        //    c.Emails.Add(em);

        //    string subject = "SupportRequest"  + AppInfos.ApplicationName;

        //    string body = "";
        //    body += "hello" + " Peter, \n\n"; 
        //    body += "\n\n";
        //    body += AppInfos.ApplicationName + "version: " + AppInfos.ApplicationVersion + Environment.NewLine;
        //    body += "System Family: " + AppInfos.SystemFamily + Environment.NewLine;
        //    body += "System Version: " + AppInfos.SystemVersion + Environment.NewLine;
        //    body += "System Architecture: " + AppInfos.SystemArchitecture + Environment.NewLine;
        //    body += "Device Manufacturer: " + AppInfos.DeviceManufacturer + Environment.NewLine;
        //    body += "Device Model: " + AppInfos.DeviceModel + Environment.NewLine;
        //    body += Environment.NewLine;

        //    body += "Last 100 Logs :" + Environment.NewLine;
        //    body += "---------------" + Environment.NewLine;
        //    foreach (LogEntry le in Dal.GetLatestXLogs(100))
        //    {
        //        body += le.LogEntryDate + " " + le.LogType + " " + le.Description + Environment.NewLine;
        //    }


        //    // For Attach File to E-Mail (Only works with App STore Mail Clients) 
        //    // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/5a539ff6-ce81-4f05-83a0-8bf430d827f8/uwpwindows-10-c-universal-windows-app-email-with-attachment?forum=wpdevelop&prof=required
        //    //StorageFile logfile = await CreateLogFile();

        //    await ComposeEmail(c, body, subject, null);
        //}

        //private async Task ComposeEmail(Windows.ApplicationModel.Contacts.Contact recipient, string messageBody, string subject, StorageFile attachmentFile)
        //{
        //    var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
        //    emailMessage.Subject = subject;
        //    emailMessage.Body = messageBody;


        //    if (attachmentFile != null)
        //    {
        //        var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);

        //        var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
        //            attachmentFile.Name,
        //            stream);

        //        emailMessage.Attachments.Add(attachment);
        //    }

        //    var email = recipient.Emails.FirstOrDefault<Windows.ApplicationModel.Contacts.ContactEmail>();
        //    if (email != null)
        //    {
        //        var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
        //        emailMessage.To.Add(emailRecipient);
        //    }

        //    await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

        //}

        //private async Task<StorageFile> CreateLogFile()
        //{
        //    // https://microsoft-programmierer.de/Details?d=1352&a=9&f=191&l=0&t=new-20.12.2015-17:24:40
        //    // and 
        //    // https://docs.microsoft.com/de-de/windows/uwp/files/quickstart-reading-and-writing-files
        //    // or 
        //    // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/5a539ff6-ce81-4f05-83a0-8bf430d827f8/uwpwindows-10-c-universal-windows-app-email-with-attachment?forum=wpdevelop
        //    try
        //    {
        //        string sfile = "log.txt";
        //        Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //        Windows.Storage.StorageFile textFile = await storageFolder.CreateFileAsync(sfile, Windows.Storage.CreationCollisionOption.ReplaceExisting);
        //        //Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(sfile);

        //        // First Step
        //        var stream = await textFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
        //        //Second Step
        //        using (var outputStream = stream.GetOutputStreamAt(0))
        //        {
        //            using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
        //            {
        //                //dataWriter.WriteString("DataWriter has methods to write to various types, such as DataTimeOffset.");
        //                foreach (LogEntry le in Dal.GetAllLogs())
        //                {
        //                    dataWriter.WriteString(le.LogEntryDate + " " + le.LogType + " " + le.Description + Environment.NewLine);
        //                }
        //                await dataWriter.StoreAsync();
        //                await outputStream.FlushAsync();
        //            }
                    
        //        }
        //        stream.Dispose(); // Or use the stream variable (see previous code snippet) with a using statement as well.

        //        return await storageFolder.GetFileAsync(sfile);
        //    }
        //    catch (Exception ex)

        //    {
        //        Dal.SaveLogEntry(LogType.Exception, "Exception in createLogFile " +ex.Message);
        //        return null;
        //    }
        //}
        #endregion
    }
}
