using MSGraph.Response;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UwpSqliteDal;
using UwpSqLiteDal;

namespace HelloWindowsIot
{
    /// <summary>
    /// Represents a saved location for use in tracking travel time, distance, and routes. 
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        private ObservableCollection<TaskFolder> taskfolder = new ObservableCollection<TaskFolder>();
        /// <summary>
        /// Gets or sets the taskfolder. 
        /// </summary>
        public ObservableCollection<TaskFolder> MyOutlookTaskFolders
        {
            get { return this.taskfolder; }
            set { this.SetProperty(ref this.taskfolder, value); }
        }
        public List<OutlookTask> MyOutlookTasks;

        private TaskFolder selectedTaskFolder;
        public TaskFolder SelectedTaskFolder
        {

            get { return this.selectedTaskFolder; }
            set { this.SetProperty(ref this.selectedTaskFolder, value); }
        }


        public SettingsViewModel()
        {
            SaveSettingsCommand = new RelayCommand(OnSaveSettings, () => isBusy);
            LoadPicsCommand = new RelayCommand(OnLoadPictureList, () => isBusy);
        }

        /// <summary>
        /// Gets the Save Settings command.
        /// </summary>
        public RelayCommand SaveSettingsCommand { get; }
        /// <summary>
        /// Gets the Add Person command.
        /// </summary>
        public RelayCommand LoadPicsCommand { get; }
        private async void OnSaveSettings()
        {
            System.Diagnostics.Debug.WriteLine("OnSaveSettings()");
            IsBusy = true;
            try
            {
                // The user needs to be signed-in
                //await _authEnforcementHandler.CheckUserAuthentication();

                IsBusy = true;
                await Dal.UpdateSetup(this.SetupSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnLoadPictureList() { }

        private Setup setupSettings;
        public Setup SetupSettings
        {
            get { return this.setupSettings; }
            set { this.SetProperty(ref this.setupSettings, value); }
        }

        private string name;
        /// <summary>
        /// Gets or sets the name 
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        private bool isBusy;
        /// <summary>
        /// True, if ViewModel is busy
        /// </summary>
        public bool  IsBusy
        {
            get { return this.isBusy; }
            set { this.SetProperty(ref this.isBusy, value); }
        }
        private string pageTitle;
        public string PageTitle
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        /// <summary>
        /// Loads the Data
        /// </summary>
        public async Task LoadData()
        {

            IsBusy = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("Get Settings From Dal ");
                //await Task.Delay(500);//TODO: Simulate Loading
                SetupSettings = await Dal.GetSetup();
                pageTitle = AppcFuncs.GetLanguage("TitleSettings");

                IList<TaskFolder> myfolderlist = await Dal.GetTaskFolderFromGraph();
                taskfolder = myfolderlist.ToObservableCollection();

                selectedTaskFolder = myfolderlist.FirstOrDefault(t => t.Id == setupSettings.TaskFolder);

                this.OnPropertyChanged("MyOutlookTaskFolders");
                this.OnPropertyChanged("SelectedTaskFolder");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error " + ex.Message); 
            }
            finally
            {
                IsBusy = false;
            }
        }
        #region ComboBoxen
        /// <summary>
        /// Pre Select Item in ComboBox TaskFolders
        /// </summary>
        /// <returns></returns>
        private async Task SelectItemInTimeBoxForChangeWallpaper()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                
            }
        }
        #endregion
    }
}
