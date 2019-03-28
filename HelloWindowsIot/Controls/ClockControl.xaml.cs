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
        public ClockControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Identified the DateTime dependency property
        /// </summary>
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime),
              typeof(ClockControl), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the CurrentTime
        /// </summary>
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }
    }
}
