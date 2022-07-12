using AppSettings;
using Microsoft.Identity.Client;
using MSGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;

namespace HelloWindowsIot
{
    public class InfoViewModel : BindableBase
    {
        private string myAppVersion;
        private string myAppName;
        private string userName;
        public string MyAppVersion
        {
            get => myAppVersion;
            set => this.SetProperty(ref myAppVersion, value);
        }
        public string MyAppName
        {
            get => myAppName;
            set => this.SetProperty(ref myAppName, value);
        }
        public string MyUsername
        {
            get => userName;
            set => this.SetProperty(ref userName, value);
        }

        public RelayCommand CreateEmail { get; private set; }
        public RelayCommand SignIn { get; private set; }
        public RelayCommand SignOut { get; private set; }


        public InfoViewModel()
        {
            CreateEmail = new RelayCommand(SendEmail, () => true);
            SignIn = new RelayCommand(LogIn, () => true);
            SignOut = new RelayCommand(LogOut, () => true);
        }


        public async Task LoadData()
        {
            MyAppVersion = "Version: " + AppInfos.ApplicationVersion;
            MyAppName = AppInfos.ApplicationName;
            LogIn();
        }

        #region login out
        private async void LogIn()
        {
           
            AuthenticationResult authResult = await GraphService.GetAuthResult();

            MyUsername = "";
            if (authResult == null)
            {
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);
                authResult = await GraphService.GetAuthResult();
            }

            MyUsername = "Hello " + authResult.Account.Username + " you signed in succesfully!" + Environment.NewLine;

            //ResultText.Text = string.Empty;
            //TokenInfoText.Text = string.Empty;

            //if (authResult != null)
            //{
            //    ResultText.Text = await GraphService.GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
            //    DisplayBasicTokenInfo(authResult);
            //    this.SignOutButton.Visibility = Visibility.Visible;
            //}

        }
        private async void LogOut()
        {
            if (await GraphService.SignOut() == true)
            {
                MyUsername = "User has signed-out";
                //this.CallGraphButton.Visibility = Visibility.Visible;
                //this.SignOutButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                //ResultText.Text = $"Error signing-out user: ";
            }
        }

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            if (authResult != null)
            {
                MyUsername += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                MyUsername += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
                MyUsername += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
            }
        }
        #endregion


        #region Email 
        private async void SendEmail()
        {
            Contact c = new Contact();
            c.FirstName = Settings.SupporterFirstName;

            ContactEmail em = new ContactEmail();
            em.Address = Settings.SupportEmail;
            c.Emails.Add(em);

            string subject = "SupportRequest" + AppInfos.ApplicationName;

            string body = "";
            body += "hello" + " Peter, \n\n";
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
            foreach (LogEntry le in DAL.AppDataBase.GetLatestXLogs(100))
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
                        foreach (LogEntry le in DAL.AppDataBase.GetAllLogs())
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
                DAL.AppDataBase.SaveLogEntry(LogType.Exception, "Exception in createLogFile " + ex.Message);
                return null;
            }
        }
        #endregion
    }
}
