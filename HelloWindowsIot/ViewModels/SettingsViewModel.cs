using System.Threading.Tasks;
using UwpSqliteDal;

namespace HelloWindowsIot
{
    /// <summary>
    /// Represents a saved location for use in tracking travel time, distance, and routes. 
    /// </summary>
    public class SettingsViewModel : BindableBase
    {

        public SettingsViewModel()
        {
            GetSetup();
        }

        private async Task GetSetup()
        {
            this.SetupSettings = await Dal.GetSetup();
        }

        private Setup setupSettings;
        public Setup SetupSettings
        {
            get { return this.setupSettings; }
            set { this.SetProperty(ref this.setupSettings, value); }
        }

        private string name;
        /// <summary>
        /// Gets or sets the name 
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}
