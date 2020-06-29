using MSGraph.Response;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.UI.Xaml;
using RWPBGTasks;
using AppSettings;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using HelloWindowsIot.Models;

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
        private string _taskResult;
        private string _taskProgress;
        #endregion

        #region Properties
        public string TaskProgress {
            get { return this._taskProgress; }
            set { this.SetProperty(ref this._taskProgress, value); }
        }
        public string TaskResult {
            get { return this._taskResult; }
            set { this.SetProperty(ref this._taskResult, value); }
        }
        public BGTaskModel MyBgTask {get;set;}

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
                this.SetupSettings.ToDoTaskListID = selectedTaskFolder.Id;
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
                    this.SetupSettings.ToDoTaskId = selectedPurchaseTask.Id;
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
            //TODO: Check if can save 
            //e.g. cant save if background tasks interval are smaller than 15 minutes 

            //if (SetupSettings.OneDrivePictureFolder=="")
            //    CanExecute = false;
            IsBusy = true;
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
                    await Task.Delay(2000);//TODO: Simulate Loading
                    await DAL.AppDataBase.UpdateSetup(this.SetupSettings);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    IsBusy = false;
                    CanExecute = true;
                }
            }
        }
        
        /// <summary>
        /// Load Settings /Setup Data for the ViewModel
        /// </summary>
        public async Task LoadData()
        {

            Settings.LoadPictureListManually = false;
            CanExecute = false;
            IsBusy = true;
            try
            {
                var ts = BGTasksSettings.ListBgTasks.Where(g => g.Name == BGTasksSettings.LoadImagesFromOneDriveTaskName).FirstOrDefault();
                if (ts != null)
                {
                    MyBgTask = ts;
                }
                    System.Diagnostics.Debug.WriteLine("Get Settings From Dal ");
                //await Task.Delay(2000);//TODO: Simulate Loading
                SetupSettings = await DAL.AppDataBase.GetSetup();
                IList<TaskFolder> myfolderlist = await DAL.AppDataBase.GetTaskFolderFromGraph();
                taskfolder = myfolderlist.ToObservableCollection();
                selectedTaskFolder = myfolderlist.FirstOrDefault(t => t.Id == setupSettings.ToDoTaskListID);
                this.OnPropertyChanged("MyOutlookTaskFolders");
                this.OnPropertyChanged("SelectedTaskFolder");

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == BGTasksSettings.LoadImagesFromOneDriveTaskName)
                    {
                        System.Diagnostics.Debug.WriteLine("Task " + task.Value.Name+  " is registriert attach handler ");
                        AttachLoadPictureListProgressAndCompletedHandlers(task.Value);
                    }
                }
                UpdateUI();
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
            //IsBusy = true;
            OnSaveSettings();
            if (MyBgTask != null)
            {

                await DAL.AppDataBase.DeleteAllPictures();
                BackgroundTaskConfig.UnregisterBackgroundTaskByName(BGTasksSettings.LoadImagesFromOneDriveTaskName);

                ApplicationTrigger trigger3 = new ApplicationTrigger();
                System.Diagnostics.Debug.WriteLine("Call RegisterBackgroundTask on Setttings ViewModel LoadPictures");
                var task = await BackgroundTaskConfig.RegisterBackgroundTask(MyBgTask.EntryPoint,
                                                                  BGTasksSettings.LoadImagesFromOneDriveTaskName,
                                                                  trigger3,
                                                                  null);
                _taskProgress = "Initializing LoadPictureList ...";
                AttachLoadPictureListProgressAndCompletedHandlers(task);

                // Reset the completion status
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values.Remove(BGTasksSettings.LoadImagesFromOneDriveTaskName);

                //Signal the ApplicationTrigger
                var result = await trigger3.RequestAsync();
                _taskResult = "Signal result: " + result.ToString();
                Settings.LoadPictureListManually = true;
                UpdateUI();
            }
        }

        private async void UpdateUI()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    OnPropertyChanged("TaskResult");
                    OnPropertyChanged("TaskProgress");
                }
                , CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachLoadPictureListProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressLoadPictures);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedLoadPictures);
        }

        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private async void OnProgressLoadPictures(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    var progress = "Progress: " + args.Progress + "%";
                    _taskProgress = progress;
                    UpdateUI();
                }
                , CoreDispatcherPriority.Normal);
        }
        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private async void OnCompletedLoadPictures(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (Settings.LoadPictureListManually == true)
            {
                //Unregister App Trigger 
                BackgroundTaskConfig.UnregisterBackgroundTaskByName(BGTasksSettings.LoadImagesFromOneDriveTaskName);
                //Register Backgroundtask 
                var apptask = await BackgroundTaskConfig.RegisterBackgroundTask(MyBgTask.EntryPoint,
                                                                           BGTasksSettings.LoadImagesFromOneDriveTaskName,
                                                                            await DAL.AppDataBase.GetTimeIntervalForTask(BGTasksSettings.LoadImagesFromOneDriveTaskName),
                                                                           null);
            }
            _taskProgress = "List Loaded";
            _taskResult = "";
            System.Diagnostics.Debug.WriteLine("OnCompleted Picturesloaded");
            Settings.LoadPictureListManually = false;
            UpdateUI();

        }

        private async Task LoadTaskList()
        {
            _isBusy = true;
            try {
                    if (SelectedTaskFolder.Id != "")
                    {
                        IList<TaskResponse> tasksinFolder = await DAL.AppDataBase.GetTasksInFolder(SelectedTaskFolder.Id);
                        System.Diagnostics.Debug.WriteLine("Must load tasks for folder : " + SelectedTaskFolder.Name);
                        taskList = tasksinFolder.ToObservableCollection();
                        
                        if (taskList.Count() !=0)
                        {
                            var ptask = tasksinFolder.FirstOrDefault(t => t.Id == SetupSettings.ToDoTaskId);
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
