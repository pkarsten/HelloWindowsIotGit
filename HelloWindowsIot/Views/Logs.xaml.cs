﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks; //Tasks
using Windows.UI.Xaml.Navigation;
using UwpSqliteDal;
using System.Collections.ObjectModel;

// https://microsoft-programmierer.de/Details?d=1347&a=9&f=181&l=0&t=UWP,-XAML,-C

namespace HelloWindowsIot
{

    public sealed partial class LogsPage : Page
    {
        public ObservableCollection<LogEntry> logList = new ObservableCollection<LogEntry>();

        
        public LogsPage()
        {
            this.InitializeComponent();
            PageTitle.Text = "Logs";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DAL.AppDataBase.SaveLogEntry(LogType.Info, "Navigated To LogsPage");
            await InitializeData(50);
        }

        #region Button Eventhandler
        private async void LoadLogs_Click(object sender, RoutedEventArgs e)
        {
            progress1.IsActive = true;
            await InitializeData(0);
        }

        private async void DeleteLogs_Click(object sender, RoutedEventArgs e)
        {
            progress1.IsActive = true;
            await DeleteLogs();
            await InitializeData(0);
        }
        #endregion

        public async Task InitializeData(int logsToLoad)
        {
            try
            {
                if (await LoadAllLogsToList(logsToLoad) == true)
                {
                    ctlLoGList.ItemsSource = logList;
                }
            }
            catch (Exception ex)
            {
                DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception InitializeData " + ex.Message);
            }
            finally
            {
                progress1.IsActive = false;
            }

        }

        private async Task<bool> LoadAllLogsToList(int logsToLoad)
        {
            try
            {
                // run the query
#if DEBUG
                DAL.AppDataBase.SaveLogEntry(LogType.Info, "Try Loading Log List ");
#endif
                if (logsToLoad > 0)
                    logList = await Task.Run(() => DAL.AppDataBase.GetLatestXLogs(logsToLoad).ToObservableCollection());
                else
                    logList = await Task.Run(() => DAL.AppDataBase.GetAllLogs().ToObservableCollection());
                return true;
            }
            catch (Exception ex)
            {
                DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception loading logs " + ex.Message);
                return false;
            }
        }

        private async Task DeleteLogs()
        {
            try
            {
                await Task.Run(() => DAL.AppDataBase.DeleteAllLogEntries());
            }
            catch (Exception ex)
            {
                DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception InitializeData " + ex.Message);
            }
            finally
            {
                progress1.IsActive = false;
            }
        }
        
    }
}
