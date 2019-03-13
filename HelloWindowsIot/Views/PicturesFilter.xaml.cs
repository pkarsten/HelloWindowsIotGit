using RWPBGTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UwpSqliteDal;
using AppSettings;

namespace HelloWindowsIot
{
    /// <summary>
    /// Page for Select Picture Filter 
    /// </summary>
    public sealed partial class PicturesFilter : Page
    {
        #region Variables
        private List<CommonFolderQueryObject> CommFolderQueryCollection = new List<CommonFolderQueryObject>();
        private List<VirtualFolderObject> VirtualFolderCollection = new List<VirtualFolderObject>();

        private CommonFolderQuery SelectedGroupFilter { get; set; }
        private string SelectedVirtualFolder { get; set; }

        private IReadOnlyList<StorageFolder> VirtualFolderResultList { get; set; }

        private IReadOnlyList<StorageFile> listWithFoundPicturesInVirtualFolder;

        private int noOfPicturesFound = 0;

        private StoreContext context = null;

        #endregion

        #region Constructor
        public PicturesFilter()
        {
            this.InitializeComponent();
            PageTitle.Text = AppcFuncs.GetLanguage("TitlePictureFilter");
        }
        #endregion

        #region Navigate
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            progress1.IsActive = true;
            Dal.SaveLogEntry(LogType.Info, "Navigated To PicturesFilter Page");

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == Settings.SearchPicturesTaskName)
                {
                    AttachSearchPictureProgressAndCompletedHandlers(task.Value);
                }
            }
            await FillCommonQueryCollection();
            comboBoxCommonFolderQueries.ItemsSource = CommFolderQueryCollection;

            await SetComboBoxCommonFolderQueries();
            progress1.IsActive = false;

            UpdateUI();
        }
        #endregion

        #region SearchPictureTaskEventhandler
        private async void RunSearchPicsTask_Click(object sender, RoutedEventArgs e)
        {
            await SaveFilterSettingsInDB();


            Settings.SearchPicturesTaskProgress = "Initializing....";

            AppSettings.RegisteredBeforeStartSearchPicManual = BackgroundTaskConfig.GetBackgroundRegisteredTaskStatus(Settings.SearchPicturesTaskName);

            ApplicationTrigger trigger3 = new ApplicationTrigger();

            BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.SearchPicturesTaskName);

            Settings.SearchPicturesTaskResult = "";

            var task = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                   Settings.SearchPicturesTaskName,
                                                                   trigger3,
                                                                   null);
            AttachSearchPictureProgressAndCompletedHandlers(task);

            // Reset the completion status
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(Settings.SearchPicturesTaskName);

            //Signal the ApplicationTrigger
            var result = await trigger3.RequestAsync();
            Settings.SearchPicturesTaskResult = "Signal result: " + result.ToString();
            AppSettings.SearchPicManual = true;
            UpdateUI();
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachSearchPictureProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressSearchPictures);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedSearchPictures);
        }
        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedSearchPictures(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (AppSettings.SearchPicManual == true)
            {
                //Unregister App Trigger 
                BackgroundTaskConfig.UnregisterBackgroundTasks(Settings.SearchPicturesTaskName);
                Settings.SearchPicturesTaskProgress = "";
                Settings.SearchPicturesTaskResult = "";

                if (AppSettings.RegisteredBeforeStartSearchPicManual == true)
                {

                    var apptask = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                           Settings.SearchPicturesTaskName,
                                                                            Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                                                                           null);
                    AttachSearchPictureProgressAndCompletedHandlers(apptask);
                    AppSettings.RegisteredBeforeStartSearchPicManual = true;

                }
                AppSettings.SearchPicManual = false;
            }
            UpdateUI();
        }
        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgressSearchPictures(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var progress = "Progress: " + args.Progress + "%";
                Settings.SearchPicturesTaskProgress = progress;
                UpdateUI();
            });
        }
        #endregion

        #region UpdateUI
        /// <summary>
        /// Update the scenario UI.
        /// </summary>
        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                PicTaskRunNowButton.IsEnabled = CheckIfCanEnablePicTaskRunNowButton();
                PicTaskProgress.Text = Settings.SearchPicturesTaskProgress;
                PicTaskResult.Text = Settings.SearchPicturesTaskResult;
            });
        }

        private bool CheckIfCanEnablePicTaskRunNowButton()
        {
            return SearchPicTaskIsInProgress() && FoundPictures();
        }

        private bool SearchPicTaskIsInProgress()
        {
            return String.IsNullOrEmpty(Settings.SearchPicturesTaskProgress);
        }

        private bool FoundPictures()
        {
            return noOfPicturesFound > 0;
        }

        private async Task SetComboBoxCommonFolderQueries()
        {
            try
            {
                var p = await GetPicFilterConfig();
                if (!String.IsNullOrEmpty(p.CommonFolderQuery))
                {
                    var s = p.CommonFolderQuery;

                    int index = CommFolderQueryCollection.FindIndex(a => a.CommonFolderEnum.ToString() == s);
                    if (index >= 0)
                    {
                        comboBoxCommonFolderQueries.SelectedItem = CommFolderQueryCollection.ElementAt(index);
                    }
                    if ((comboBoxCommonFolderQueries.SelectedItem == null) || index < 0)
                        comboBoxCommonFolderQueries.SelectedIndex = 0;
                }
                else
                {
                    //Select First Entry 
                    comboBoxCommonFolderQueries.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  SetComboBoxCommonFolderQueries() " + ex.Message);
            }
        }

        private async Task SetComboBoxVirtualFolders()
        {
            try
            {
                var p = await GetPicFilterConfig();
                if (!String.IsNullOrEmpty(p.VirtualFolder))
                {
                    var s = p.VirtualFolder;

                    int index = VirtualFolderCollection.FindIndex(a => a.VirtualFolderName == s);
                    if (index >= 0)
                    {
                        comboBoxVirtualFolders.SelectedItem = VirtualFolderCollection.ElementAt(index);
                    }

                    if ((comboBoxVirtualFolders.SelectedItem == null) || index < 0)
                        comboBoxVirtualFolders.SelectedIndex = 0;
                }
                else
                {
                    //Select First Entry 
                    comboBoxVirtualFolders.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  SetComboBoxVirtualFolders() " + ex.Message);
            }
        }
        #endregion

        #region Eventhandler
        /// <summary>
        /// When ChangeSelection in ComboBox that holds the CommFolderQueryCollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void comboBoxCommonFolderQueries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                progress1.IsActive = true;
                CommonFolderQueryObject selectedGroupFilter = comboBoxCommonFolderQueries.SelectedItem as CommonFolderQueryObject;
                SelectedGroupFilter = selectedGroupFilter.CommonFolderEnum;

                await FillVirtualFolderCollection();
                comboBoxVirtualFolders.ItemsSource = VirtualFolderCollection;

                await SetComboBoxVirtualFolders();

                comboBoxVirtualFolders.IsEnabled = true;
                UpdateUI();
                progress1.IsActive = false;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  comboBoxCommonFolderQueries_SelectionChanged() " + ex.Message);
            }
        }

        /// <summary>
        /// When ChangeSelection in the ComboBox thats holds the VirtualFolderCollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void comboBoxVirtualFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                progress1.IsActive = true;
                VirtualFolderObject selectedVirtualFolder = comboBoxVirtualFolders.SelectedItem as VirtualFolderObject;
                if (selectedVirtualFolder != null)
                {
                    SelectedVirtualFolder = selectedVirtualFolder.VirtualFolderName;
                    noOfPicturesFound = await CountPicturesInVirtualFolder();
                }
                UpdateUI();
                progress1.IsActive = false;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  comboBoxVirtualFolders_SelectionChanged() " + ex.Message);
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// Fills a list with Possible CommonFolderQuery Objects
        /// </summary>
        /// <returns></returns>
        private async Task FillCommonQueryCollection()
        {
            try
            {
                List<CommonFolderQueryObject> tempList = new List<CommonFolderQueryObject>();

                VirtualFolderCollection.Clear();

                var list = await AppSettings.GetAllowedPicFilterList();
                var allowedFilters = list.Where(f => f.Value == true);

                foreach (var entry in allowedFilters)
                {
                    CommonFolderQueryObject mycfqo = new CommonFolderQueryObject();
                    mycfqo.ReadableFilterGroupName = entry.Key.ReadableName();
                    mycfqo.CommonFolderEnum = entry.Key;
                    tempList.Add(mycfqo);
                }

                CommFolderQueryCollection = tempList;
            }
            catch(Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  FillCommonQueryCollection() " + ex.Message);
            }
        }

        /// <summary>
        /// Fill List with Virtual Folders thar are Result from CreateFolderQuery
        /// </summary>
        /// <returns></returns>
        private async Task FillVirtualFolderCollection()
        {
            try
            {
                List<VirtualFolderObject> tempList = new List<VirtualFolderObject>();

                // Get/Set Storage Folder with Picture Library 
                StorageFolder picturesLib = KnownFolders.PicturesLibrary;

                // Create Group Query over the Picture Library 
                StorageFolderQueryResult queryResult = picturesLib.CreateFolderQuery(SelectedGroupFilter);

                // Query Result with Virtual Folders (groups) e.g. 5 Star, 4 Star, 3 Stars,... No in a Group , Tag etc 
                VirtualFolderResultList = await queryResult.GetFoldersAsync();

                foreach (StorageFolder fol in VirtualFolderResultList)
                {
                    VirtualFolderObject mvfo = new VirtualFolderObject();
                    mvfo.VirtualFolderName = fol.DisplayName;
                    tempList.Add(mvfo);
                }
                VirtualFolderCollection = tempList;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  FillVirtualFolderCollection() " + ex.Message);
            }
        }

        /// <summary>
        /// Count Pictures Found in Virtual Folder
        /// </summary>
        /// <returns></returns>
        private async Task<int> CountPicturesInVirtualFolder()
        {
            try
            {
                int countPics = 0;
                if (VirtualFolderResultList != null)
                {

                    if (SelectedVirtualFolder == "")
                    {
                        //Get First Folder (best rated) 
                        listWithFoundPicturesInVirtualFolder = await VirtualFolderResultList.First().GetFilesAsync();
                    }
                    else
                    {
                        listWithFoundPicturesInVirtualFolder = await VirtualFolderResultList.Where(f => f.DisplayName == SelectedVirtualFolder).First().GetFilesAsync();
                    }
                    countPics = listWithFoundPicturesInVirtualFolder.Count();
                    txtTotalPicsFound.Visibility = Visibility.Visible;
                    txtTotalPicsFound.Text = String.Format(AppSettings.AppResourceMap.GetValue("foundPictures", ResourceContext.GetForCurrentView()).ValueAsString, listWithFoundPicturesInVirtualFolder.Count.ToString());
                }
                return countPics;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  CountPicturesInVirtualFolder() " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Save Filter in Database
        /// </summary>
        /// <returns></returns>
        private async Task SaveFilterSettingsInDB()
        {
            try
            {
                progress1.IsActive = true;
                PicFilter filterConfig = Dal.GetPicFilter();
                filterConfig.CommonFolderQuery = SelectedGroupFilter.ToString();
                filterConfig.VirtualFolder = SelectedVirtualFolder;
                Dal.UpdatePicFilterConfig(filterConfig);
                progress1.IsActive = false;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in  SaveFilterSettingsInDB() " + ex.Message);
            }
        }

        private async Task<PicFilter> GetPicFilterConfig()
        {
                return Dal.GetPicFilter();
        }


        #endregion

        #region Store
        public async void GetLicenseInfo()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            progress1.IsActive = true;
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();
            progress1.IsActive = false;

            if (appLicense == null)
            {
                storeTextBlock.Visibility = Visibility.Visible;//errRetrievingLicence
                storeTextBlock.Text = AppcFuncs.GetLanguage("errRetrievingLicence"); ///"An error occurred while retrieving the license."; 
                return;
            }

            // Use members of the appLicense object to access license info...

            // Access the add on licenses for add-ons for this app.
            foreach (KeyValuePair<string, StoreLicense> item in appLicense.AddOnLicenses)
            {
                StoreLicense addOnLicense = item.Value;
                // Use members of the addOnLicense object to access license info
                // for the add-on...
            }
        }
        #endregion

    }
}
