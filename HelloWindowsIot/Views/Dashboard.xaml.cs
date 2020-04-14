using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RWPBGTasks;
using Windows.ApplicationModel.Background;
using AppSettings;
using MSGraph;

namespace HelloWindowsIot
{
    public sealed partial class DashBoard : Page
    {
        public DashBoardViewModel ViewModel { get; set; }

        public DashBoard()
        {
            this.InitializeComponent();
            this.ViewModel = new DashBoardViewModel();
        }

        

      

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachChangeImageProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            //task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            //task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }

        /// <summary>
        /// Loads the saved Dashboard data on first navigation
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
                var dashboarddata = await SampleDashBoardData.GetSampleDashBoardDataAsync();
                await ViewModel.LoadData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
