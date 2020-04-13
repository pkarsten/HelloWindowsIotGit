using AppSettings;
using RWPBGTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;


namespace HelloWindowsIot
{
    /// <summary>
    /// The Main Page
    /// </summary>
    public partial class MainPage : Page
    {
        // GlyphChar="\xE71C" 0 (Filter Symbol)
        // GlyphChar="\xE8BA" (Status Symbol)
        // GlyphChar="\xE80F" (House Symbol)
        List<Scenario> topScenarios = new List<Scenario>
        {
            new Scenario() { Title="Dashboard", ClassType=typeof(DashBoard), GlyphChar="\xE80F"},
            new Scenario() { Title="Settings", ClassType=typeof(SettingsPage), GlyphChar="\xE713"},
            new Scenario() { Title="Logs", ClassType=typeof(LogsPage), GlyphChar="\xE8F1"}
            
        };
        List<Scenario> bottomScenarios = new List<Scenario>
        {
            new Scenario() { Title="About", ClassType=typeof(InfoPage), GlyphChar="\xE77B"}
        };
    }
}

