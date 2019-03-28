using System;
using Windows.UI.Xaml;

namespace HelloWindowsIot
{
    public class ClockViewModel: BindableBase
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        public DateTime CurrentTime { get { return DateTime.Now; } }
        public ClockViewModel()
        {
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private bool enableClock;
        public bool EnableClock
        {
            get { return this.enableClock; }
            set { this.SetProperty(ref this.enableClock, value); }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (enableClock == false)
                _timer.Stop();
            else
                this.OnPropertyChanged("CurrentTime");
        }
    }
}
