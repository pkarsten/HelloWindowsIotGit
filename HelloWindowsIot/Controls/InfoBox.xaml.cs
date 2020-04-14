using MSGraph;
using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HelloWindowsIot.Controls
{
    public sealed partial class InfoBox : UserControl
    {
        public InfoBoxViewModel ViewModel { get; set; }
        public InfoBox()
        {
            System.Diagnostics.Debug.WriteLine("InfoBox InfoBoxInfoBoxInfoBoxInfoBoxInfoBoxInfoBoxInfoBoxInfoBoxInfoBox");
            this.InitializeComponent();
            this.ViewModel = new InfoBoxViewModel();
        }

        public InfoModel IM 
        {
            get => (InfoModel)GetValue(IM_Property);
            set{SetValue(IM_Property, value); this.ViewModel.PIM = value; System.Diagnostics.Debug.WriteLine("INFO INFO : " + value.TotalPicsinDB); }
        }

        public static readonly DependencyProperty IM_Property =
            DependencyProperty.Register("IM_Property", typeof(InfoModel), typeof(InfoBox), new PropertyMetadata(false));

        public string t1
        {
            get => (string)GetValue(t1_Property);
            set { SetValue(t1_Property, value); System.Diagnostics.Debug.WriteLine("INFO t1 INFO : " + value); }
        }

        public static readonly DependencyProperty t1_Property =
            DependencyProperty.Register("t1_Property", typeof(string), typeof(InfoBox), new PropertyMetadata(false));
    }
}
