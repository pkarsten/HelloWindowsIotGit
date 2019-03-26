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
    public static class AppSettings
    {
        // Our settings
        public static List<TimeObject> ChangeWallpaperTimeCollection { get; } = new List<TimeObject> {
                 new TimeObject { TName = "15 seconds", TSeconds = 15 },//TODO Language
                 new TimeObject { TName = "30 seconds", TSeconds = 30 },
                 new TimeObject { TName = "60 seconds", TSeconds = 60 },
                 new TimeObject { TName = "1 "+AppcFuncs.GetLanguage("txtDay"), TMinutes = 1440 }
            };

        public static List<TimeObject> SearchTimeCollection { get; } = new List<TimeObject> {
                 new TimeObject { TName = "1 "+AppcFuncs.GetLanguage("txtHour"), TMinutes = 60 },
                 new TimeObject { TName = "1 "+AppcFuncs.GetLanguage("txtDay"), TMinutes = 1440 },
                 new TimeObject { TName = "1 "+AppcFuncs.GetLanguage("txtWeek"), TMinutes = 10080 }
            };
        public static bool SearchPicManual { get; set; }
        public static bool RegisteredBeforeStartSearchPicManual { get; set; }
        public static bool RegisteredBeforeStartLoadPicturesFromOneDrive{ get; set; }





        public static ResourceMap AppResourceMap { get; } = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");



        public static async Task<Dictionary<CommonFolderQuery, bool>> GetAllowedPicFilterList()
        {
            var lf = await AppcFuncs.LizenzInfos();

            System.Diagnostics.Debug.WriteLine("GetAllowedPicFilterList Licence is " + lf.Message);
            if (lf.IsTrial)
            {
                return Settings.AllowedTrialVersionPicFilterList;
            }
            else
            {
                return Settings.AllowedFullVersionPicFilterList;
            }
        }
    }

    public partial class MainPage : Page
    {
        List<Scenario> topScenarios = new List<Scenario>
        {
            new Scenario() { Title="Settings", ClassType=typeof(SettingsPage), GlyphChar="\xE713"},
            new Scenario() { Title="Dashboard", ClassType=typeof(DashBoard), GlyphChar="\xE713"},
            new Scenario() { Title="Desktop", ClassType=typeof(GraphDemo), GlyphChar="\xE713"},
            new Scenario() { Title=AppcFuncs.GetLanguage("TitleStartPage"), ClassType=typeof(StartPage), GlyphChar="\xE80F"}, // use "\xE713" instead of " &#xE713;"
            new Scenario() { Title=AppcFuncs.GetLanguage("TitlePictureFilter"), ClassType=typeof(PicturesFilter), GlyphChar="\xE71C"},
            new Scenario() { Title=AppcFuncs.GetLanguage("TitleTaskOverview"), ClassType=typeof(StatusPage), GlyphChar="\xE8BA"},
            new Scenario() { Title=AppcFuncs.GetLanguage("TitleLogs"), ClassType=typeof(LogsPage), GlyphChar="\xE8F1"}
            
        };
        List<Scenario> bottomScenarios = new List<Scenario>
        {
            new Scenario() { Title=AppcFuncs.GetLanguage("TitleSupport"), ClassType=typeof(InfoPage), GlyphChar="\xE77B"}
        };
    }
}

