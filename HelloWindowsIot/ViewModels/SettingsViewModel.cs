using MSGraph.Response;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UwpSqliteDal;
using UwpSqLiteDal;
using MSGraph;

namespace HelloWindowsIot
{
    /// <summary>
    /// Represents App Settings Setup 
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        #region Fields
        private ObservableCollection<TaskFolder> taskfolder = new ObservableCollection<TaskFolder>();
        private TaskFolder selectedTaskFolder;
        private ObservableCollection<TaskResponse> taskList = new ObservableCollection<TaskResponse>();
        private TaskResponse selectedPurchaseTask;
        private bool canExecute;
        private Setup setupSettings;
        private bool _isBusy;
        #endregion

        #region Properties
        public Setup SetupSettings
        {
            get { return this.setupSettings; }
            set { this.SetProperty(ref this.setupSettings, value);}
        }
        public ObservableCollection<TaskFolder> MyOutlookTaskFolders
        {
            get { return this.taskfolder; }
            set { this.SetProperty(ref this.taskfolder, value); }
        }
        public TaskFolder SelectedTaskFolder
        {

            get { return this.selectedTaskFolder; }
            set
            {
                this.SetProperty(ref this.selectedTaskFolder, value);
                //handle your "event" here... 
                //h ttps://social.msdn.microsoft.com/Forums/sqlserver/en-US/c286f324-50fb-4641-a0d0-b36258de3847/uwp-xbind-event-handling-and-mvvm?forum=wpdevelop
                System.Diagnostics.Debug.WriteLine("Selected Item " + selectedTaskFolder.Name + " id " + selectedTaskFolder.Id);
                this.SetupSettings.TaskFolder = selectedTaskFolder.Id;
                if (!string.IsNullOrEmpty(selectedTaskFolder.Id))
                {
                    LoadTaskList();
                }
            }
        }
        public ObservableCollection<TaskResponse> MyOutlookTasks
        {
            get { return this.taskList; }
            set { this.SetProperty(ref this.taskList, value); }
        }
        public TaskResponse SelectedPurchaseTask
        {

            get { return this.selectedPurchaseTask; }
            set
            {
                this.SetProperty(ref this.selectedPurchaseTask, value);
                //handle your "event" here... 
                //System.Diagnostics.Debug.WriteLine("Selected Task " + selectedPurchaseTask.Subject+ " id " + selectedPurchaseTask.Id);
                if(this.selectedPurchaseTask != null)
                    this.SetupSettings.PurchaseTaskID = selectedPurchaseTask.Id;
            }
        }
        /// <summary>
        /// True, if can Save Settings 
        /// </summary>
        public bool CanExecute
        {
            get => this.canExecute;
            set => this.SetProperty(ref this.canExecute, value);
        }
        /// <summary>
        /// True, if ViewModel is busy, for Show Progress / Load Ring
        /// </summary>
        public bool IsBusy
        {
            get { return this._isBusy; }
            set { this.SetProperty(ref this._isBusy, value); }
        }
        public RelayCommand Submit { get; private set; }
        public RelayCommand LoadPicsCommand { get; private set; }
        #endregion

        #region Constructor
        public SettingsViewModel()
        {
            System.Diagnostics.Debug.WriteLine("Initialize SettingsViewModel ");
            LoadPicsCommand = new RelayCommand(LoadPictureList, () =>true);
            Submit = new RelayCommand(OnSaveSettings, () => true);
        }
        #endregion

        #region Events / Functions
        /// <summary>
        /// Save Settings in Database if CanExecute = true
        /// </summary>
        private async void OnSaveSettings()
        {
            //if (SetupSettings.OneDrivePictureFolder=="")
            //    CanExecute = false;

            if (CanExecute == false)
            {
                System.Diagnostics.Debug.WriteLine("Can't save Settings");
                return;
            }
            else
            {

                System.Diagnostics.Debug.WriteLine("OnSaveSettings()");
                //IsBusy = true; //Causes : StackOverflowException 
                try
                {
                    //IsBusy = true; // => StackOverflowException 
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
        }
        
        /// <summary>
        /// Load Settings /Setup Data for the ViewModel
        /// </summary>
        public async Task LoadData()
        {

            CanExecute = false;
            IsBusy = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("Get Settings From Dal ");
                //await Task.Delay(2000);//TODO: Simulate Loading
                SetupSettings = await Dal.GetSetup();
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
                CanExecute = true;
            }
        }

        /// <summary>
        /// Loads Picturelist from Given OneDrive Folder and Save it in DataBase
        /// </summary>
        /// <returns></returns>
        private async void LoadPictureList()
        {
            _isBusy = true;
            canExecute = false;
            OnSaveSettings();
            //TODO: Run this on Backgroundtask and notify progress on UI because when run blocks the UI 
            await Dal.LoadImagesFromOneDriveInDBTable(SetupSettings.OneDrivePictureFolder);
            _isBusy = false;
            canExecute= true;
        }

        private async Task LoadTaskList()
        {
            _isBusy = true;
            try {
                    if (SelectedTaskFolder.Id != "")
                    {
                        IList<TaskResponse> tasksinFolder = await Dal.GetTasksInFolder(SelectedTaskFolder.Id);
                        System.Diagnostics.Debug.WriteLine("Must load tasks for folder : " + SelectedTaskFolder.Name);
                        taskList = tasksinFolder.ToObservableCollection();
                        
                        if (taskList.Count() !=0)
                        {
                            var ptask = tasksinFolder.FirstOrDefault(t => t.Id == SetupSettings.PurchaseTaskID);
                            if (ptask != null)
                            {
                                selectedPurchaseTask = ptask;
                            }
                            else
                            {
                            selectedPurchaseTask = tasksinFolder.FirstOrDefault();
                            }
                        this.OnPropertyChanged("MyOutlookTasks");
                        this.OnPropertyChanged("SelectedPurchaseTask");
                    }
                    }
                }
                catch(Exception ex)
                {
                System.Diagnostics.Debug.WriteLine("Exception in LoadTaskList : " + ex.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }
        #endregion
    }

}
