using AppSettings;
using RWPBGTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MSGraph;

namespace HelloWindowsIot
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain()....
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Initialize the App launch.
        /// </summary>
        /// <returns>The AppShell of the app.</returns>
        private async Task Initialize()
        {

            await DAL.AppDataBase.InitializeAsync();
            await BackgroundTaskConfig.UnRegisterAllTasks();
            await BackgroundTaskConfig.RegisterNeededTasks();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await Initialize();

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Don't Repeat App Initialisierung, if the window always contains content
            // stay secure the windows is active
            if (rootFrame == null)
            {
                // Frame, for the navigation context and for parameter to g/navigate to the first page 
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // For load previous state of the app 
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                // Place frame in current windows 
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    var accessToken = await GraphService.GetTokenForUserAsync();

                    if (accessToken != null)
                    {
                        //TODO: Check if Settings are Ok for Run the App, then run DashBoard, other else run the SettingsPage and say what it's wrong
                        rootFrame.Navigate(typeof(DashBoard), e.Arguments);
                    }
                    else
                        rootFrame.Navigate(typeof(InfoPage), e.Arguments);
                }
                else
                {
                    var page = rootFrame.Content as MainPage;
                }

                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();

                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Application Launched");
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            await BackgroundTaskConfig.UnRegisterAllTasks();
            deferral.Complete();
        }
    }
}
