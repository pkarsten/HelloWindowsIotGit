using System;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement; //ApplicationView
using Windows.ApplicationModel.Core; //CoreApplicationViewTitleBar
using Windows.ApplicationModel.Background; //BackgroundTasks
using Windows.Media.Playback; //BackgroundMediaPlayer
using System.Threading.Tasks; //Tasks
using Windows.UI.Popups; //Messagebox MessageDialog
using RWPBGTasks;
using Windows.UI.Core;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Windows.Input;
using Windows.Globalization;
using UwpSqliteDal;

namespace HelloWindowsIot
{

    public sealed partial class MainPage : Page
    {
        #region Variables
        // A pointer to the ApplicationTrigger so we can signal it later
        public static MainPage Current;
        public List<Scenario> TopScenarios
        {
            get { return this.topScenarios; }
        }
        public List<Scenario> BottomScenarios
        {
            get { return this.bottomScenarios; }
        }



        #endregion

        #region Constructor
        public MainPage()
        {
            InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;
            Header.Text = AppInfos.ApplicationName + " " + AppInfos.ApplicationVersion;

            //TODO: Remove Test Language 
            //ApplicationLanguages.PrimaryLanguageOverride = "de-DE";
        }
        #endregion

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
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
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        #region scenarios
        /// <summary>
        /// Called whenever the user changes selection in the scenarios list.  This method will navigate to the respective
        /// sample scenario page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the status block when navigating scenarios.
            NotifyUser(String.Empty, NotifyType.StatusMessage);

            ListBox scenarioListBox = sender as ListBox;
            Scenario s = scenarioListBox.SelectedItem as Scenario;
            if (s != null)
            {
                /// I want Dashboard in "Fullscreen" without Splitter and Scenario Control and so on ... 
                if (s.ClassType == typeof(DashBoard))
                {
                    this.Frame.Navigate(typeof(DashBoard));
                }
                else
                {
                    ScenarioFrame.Navigate(s.ClassType);
                    if (Window.Current.Bounds.Width < 640)
                    {
                        Splitter.IsPaneOpen = false;
                    }
                    FooterControl.SelectedItem = null;
                    ScenarioControl.SelectedItem = scenarioListBox.SelectedItem;
                }
            }
        }

         private void FooterControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
                    ListBox scenarioListBox = sender as ListBox;
                    Scenario s = scenarioListBox.SelectedItem as Scenario;
                    if (s != null)
                    {
                        ScenarioFrame.Navigate(s.ClassType);
                        if (Window.Current.Bounds.Width < 640)
                        {
                            Splitter.IsPaneOpen = false;
                        }
                        ScenarioControl.SelectedItem = null;
                        FooterControl.SelectedItem = scenarioListBox.SelectedItem;
                    }


         }

        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            //await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }
        #endregion

        #region Navigate
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Populate the scenario list from the AppConfiguration.cs file
            // Scenarios for Top in Hamburger Menu
            ScenarioControl.ItemsSource = topScenarios;
            ScenarioControl.SelectedIndex = 0;

            // Scenarios for Bottom in Hamburger Menu
            FooterControl.ItemsSource = bottomScenarios;
            FooterControl.SelectedIndex = -1;
        }
        #endregion
    }
}
