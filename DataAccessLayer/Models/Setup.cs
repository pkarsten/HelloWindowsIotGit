using SQLite.Net.Attributes;
using System;
using Windows.Storage;

namespace UwpSqliteDal
{
    /// <summary>
    /// Represents Setup Entry in Table  
    /// </summary>
    public sealed class Setup
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        #region Logging
        /// <summary>
        /// Gets or sets If Logging is Enable
        /// </summary>
        public bool EnableLogging{ get; set; }
        #endregion

        #region PictureSHow
        /// <summary>
        /// Get or sets the OneDrive Folder Path where app will look/search for images
        /// </summary>
        public string OneDrivePictureFolder { get; set; }

        /// <summary>
        /// get or sets the Interval (in seconds) for the picture Diashow on the Dashboard
        /// </summary>
        public int IntervalForDiashow { get; set; }

        /// <summary>
        /// Get or Sets if PictureSHow should be run or not 
        /// </summary>
        public bool EnablePictureShow { get; set; }
        #endregion

        #region Tasks
        /// <summary>
        /// get or sets the Task Folder Path where we are looking for Outlook Tasks
        /// </summary>
        public string TaskFolder { get; set; }

        /// <summary>
        /// Gets or sets the ID which represents the Purchase Task 
        /// </summary>
        public string PurchaseTaskID { get; set; }

        /// <summary>
        /// get or sets if Purchase Task should be seen on Dashboard
        /// </summary>
        public bool EnablePurchaseTask { get; set; }
        #endregion

        #region Calendar
        /// <summary>
        /// get or sets the Days from today we would list the calendar events 
        /// </summary>
        public int NextEventDays { get; set; }

        /// <summary>
        /// get or sets if Calendar Add On should be enabled or disabled
        /// </summary>
        public int EnableCalendarAddon { get; set; }

        /// <summary>
        /// get or sets if CalendarNext Events should be seen on dashboard 
        /// </summary>
        public bool EnableCalendarNextEvents { get; set; }

        /// <summary>
        /// get or sets if calendar today events hould be seen on Dashboard 
        /// </summary>
        public bool EnableTodayEvents { get; set; }
        #endregion

        #region Clock
        /// <summary>
        /// get or sets if Clock AddOn must be enable or disable on dashboard
        /// </summary>
        public bool EnableClock { get; set; }
        #endregion


        #region not needed?
        //TODO: Check this 
        /// <summary>
        /// Time Interval in Minutes for run the Background Task Search Pictures 
        /// </summary>
        public int IntervalForSearchPictures { get; set; }

        /// <summary>
        /// Time Interval in Minutes for run the Background Task Change WallPaper
        /// </summary>
        public int IntervalForChangeWallPaper { get; set; }
        #endregion


    }
}
