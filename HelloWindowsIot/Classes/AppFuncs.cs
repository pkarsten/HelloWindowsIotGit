using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Services.Store;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using RWPBGTasks;

namespace HelloWindowsIot
{
    public static class AppcFuncs
    {
        #region JumpList
        public static async Task SetupJumpList()
        {
            if (JumpList.IsSupported())
            {
                JumpList jumpList = await JumpList.LoadCurrentAsync();
                jumpList.Items.Clear();

                JumpListItem changeWPItem = JumpListItem.CreateWithArguments("changeWP", "Change Wallpaper"); //TODO: Language
                changeWPItem.Logo = new Uri("ms-appx:///Assets/Icons/smalltile-sdk.png");
                //JumpListItem videoItem = JumpListItem.CreateWithArguments("video", "Capture video");
                //videoItem.Logo = new Uri("ms-appx:///Assets/video.png");
                jumpList.Items.Add(changeWPItem);
                //jumpList.Items.Add(videoItem);

                await jumpList.SaveAsync();
            }
        }
        #endregion

        #region localisation

        //For Translate go to Folder MultilingualResources, Right muse click open with "Multilanguage editor" //TODO: Add to Coder Info txt Page 


        //See http://www.codeproject.com/Articles/862152/Localization-in-Windows-Universal-Apps
        // or http://stackoverflow.com/questions/36840231/how-to-use-resources-resw-in-class-library-project-in-uwp
        /// <summary>
        /// Get translated Text from Resources 
        /// if resources has placeholders use GetLanguageWithPlaceholder instead of GetLanguage(string resourcename) 
        /// </summary>
        /// <param name="resourcename"></param>
        /// <returns></returns>
        public static string GetLanguage(string resourcename)
        {
            ResourceCandidate resource1 = ResourceManager.Current.MainResourceMap.GetValue("Resources/"+ resourcename, ResourceContext.GetForCurrentView());
            return resource1.ValueAsString;
        }

        /// <summary>
        /// if resources has placeholders use this instead of GetLanguage(string resourcename) 
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="strreplace"></param>
        /// <returns></returns>
        public static string GetLanguageWithPlaceholder(string resourcename)
        {
            //resourcename =" hallo {0}";
            // String.Format(AppSettings.AppResourceMap.GetValue("resourcename", ResourceContext.GetForCurrentView()).ValueAsString, $placeholdercontent)
            return "";
        }
        #endregion

        #region Store
        // See https://docs.microsoft.com/de-de/windows/uwp/monetize/implement-a-trial-version-of-your-app
        private static StoreAppLicense _SAppLicense = null;

        public static StoreAppLicense SAppLicense { get; set; }
        public static StoreContext SContext = null;

        public async static void InitializeLicence()
        {
            if (SContext == null)
            {
                SContext = StoreContext.GetDefault();
            }
            SAppLicense = await SContext.GetAppLicenseAsync();
        }

        public static async Task<Lizenz> LizenzInfos()
        {
            Lizenz _lizenz = new Lizenz();
            try
            {
                if (AppcFuncs.SContext == null)
                    InitializeLicence();

                Dal.SaveLogEntry(LogType.Info, "Get LizenzInfos() ");

                StoreAppLicense license = await AppcFuncs.SContext.GetAppLicenseAsync();

                if (license.IsActive)
                {
                    if (license.IsTrial)
                    {
                        _lizenz.IsTrial = license.IsTrial;
                        _lizenz.IsActive = license.IsActive;
                        _lizenz.Message = AppcFuncs.GetLanguage("trialVersion"); 
                    }
                    else
                    {
                        _lizenz.IsTrial = license.IsTrial;
                        _lizenz.IsActive = license.IsActive;
                        _lizenz.Message = AppcFuncs.GetLanguage("fullVersion");

                    }
                }
                else
                {
                    _lizenz.IsTrial = license.IsTrial; 
                    _lizenz.IsActive = license.IsActive;
                    _lizenz.Message = AppcFuncs.GetLanguage("inactiveLicence");
                }

                if (Settings.TestTrial == true)
                {
                    _lizenz.IsTrial = true;
                    _lizenz.IsActive = true;
                    _lizenz.Message = AppcFuncs.GetLanguage("trialVersion");
                 }
                _lizenz.ExpirationDate = license.ExpirationDate;
            }
            catch (Exception ex)
            {
            }
            return _lizenz;
        }
        #endregion
    }
}
