using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background; //BackgroundTasks
using System.Threading.Tasks; //Tasks
using UwpSqliteDal;
using Windows.UI.Core;
using Windows.Storage;
using System.Collections.ObjectModel;
using AppSettings;
using RWPBGTasks;
using UwpSqLiteDal;
using MSGraph.Response;

namespace HelloWindowsIot
{
    /// <summary>
    /// Settings Page 
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        /// <summary>
        /// Gets or sets the Settingsdata . 
        /// </summary>
        public SettingsViewModel ViewModel { get; set; }


        public SettingsPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();
        }

        #region Navigate
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await HelloWindowsIotDataBase.SaveLogEntry(LogType.Info, "Navigated To SettingsPage");
            await ViewModel.LoadData();
        }
        #endregion
    }
}
