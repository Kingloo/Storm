using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace Storm.Wpf.Common
{
    public class DispatcherCountdownTimer
    {
        #region Fields
        private readonly DateTime created = DateTime.Now;
        private readonly Action tick = null;
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
        #endregion

        #region Properties
        public bool IsRunning
            => timer != null && timer.IsEnabled;

        public TimeSpan TimeLeft
            => IsRunning ? ((created + timer.Interval) - DateTime.Now) : TimeSpan.Zero;
        #endregion
        
        public DispatcherCountdownTimer(TimeSpan span, Action tick)
        {
            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s == 10,000,000 ticks
            if (span.Ticks < (10_000 * 1000))
            {
                throw new ArgumentOutOfRangeException("span.Ticks cannot be less than 1 second", nameof(span));
            }

            this.tick = tick ?? throw new ArgumentNullException(nameof(tick));

            timer.Interval = span;
            timer.Tick += Timer_Tick;
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            tick();
            
            Stop();
        }

        public void Start() => timer.Start();

        public void Stop()
        {
            if (IsRunning)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;

                timer = null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var cc = CultureInfo.CurrentCulture;

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(cc, "Created at: {0}", created.ToString(cc)));
            sb.AppendLine(IsRunning ? "Is Running: true" : "Is Running: false");
            sb.AppendLine(string.Format(cc, "Time left: {0}", TimeLeft.ToString()));

            return sb.ToString();
        }
    }
}