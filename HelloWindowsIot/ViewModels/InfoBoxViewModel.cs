using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWindowsIot
{
    public class InfoBoxViewModel : BindableBase
    {

        private InfoModel pim;
        public InfoModel PIM
        {
            get { return this.pim; }
            set { this.SetProperty(ref this.pim, value); }
        }

        private string totPics;
        public string TotPics
        {
            get { return this.totPics; }
            set { this.SetProperty(ref this.totPics, value); }
        }
    }
}
