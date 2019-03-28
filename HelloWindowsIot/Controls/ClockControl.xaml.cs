using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace HelloWindowsIot.Controls
{
    public sealed partial class ClockControl : UserControl
    {
        public ClockViewModel ViewModel { get; set; }

        public bool EnableClock
        {
            get => (bool)GetValue(EnableClockProperty);
            set => SetValue(EnableClockProperty, value);
        }
        public static readonly DependencyProperty EnableClockProperty =
            DependencyProperty.Register("EnableClockProperty", typeof(bool), typeof(ClockControl), new PropertyMetadata(false));

        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(DepCurrTimeProperty);
            set => SetValue(DepCurrTimeProperty, value);
        }
        public static readonly DependencyProperty DepCurrTimeProperty =
            DependencyProperty.Register("DepCurrTimeProperty", typeof(DateTime), typeof(ClockControl), new PropertyMetadata(false));

        public ClockControl()
        {
            this.InitializeComponent();
            //this.ViewModel = new ClockViewModel();
            //this.ViewModel.CurrentTime = DepCurrTime;
        }
    }
}
