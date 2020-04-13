using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWindowsIot.Models
{
    /// <summary>
    /// Model for BG Task 
    /// </summary>
    public class BGTaskModel
    {
        public string Name { get; set; }
        /// <summary>
        /// NameSpace.
        /// </summary>
        public string EntryPoint { get; set; }
        public string Result { get; set; }
        public string Progress { get; set; }
        public bool Registered { get; set; }
    }
}
