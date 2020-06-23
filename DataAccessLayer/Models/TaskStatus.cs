using SQLite;
using System;
using Windows.Storage;

namespace UwpSqliteDal
{
    public sealed class BGTask
    {
        /// <summary>
        /// Gets or sets the TaskStatus.
        /// </summary>
        [PrimaryKey]
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or Sets the Registration Status Before Servicing Complete rans 
        /// If this is true, that doesn't mean that Task is really registered, this is for remember if after Update tasks must re registered (Start) 
        /// </summary>
        public bool CurrentRegisteredStatus { get; set; }

        /// <summary>
        /// Gets or Sets Additional Status 
        /// </summary>
        public string AdditionalStatus { get; set; }

        /// <summary>
        /// Gets or sets last date time , that this task runs
        /// </summary>
        public string LastTimeRun { get; set; }

    }
}
