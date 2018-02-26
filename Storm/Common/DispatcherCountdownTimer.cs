using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace Storm.Common
{
    public class DispatcherCountdownTimer
    {
        #region Fields
        private readonly DateTime created = DateTime.Now;
        private readonly Action tick = null;
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
        #endregion

        #region Properties
        public bool IsCountdownRunning => timer != null;

        public TimeSpan TimeLeft
            => IsCountdownRunning ? ((created + timer.Interval) - DateTime.Now) : TimeSpan.Zero;
        #endregion
        
        public DispatcherCountdownTimer(TimeSpan span, Action tick)
        {
            // 10,000 ticks in 1 ms => 10,000 * 1000 ticks in 1 s == 10,000,000 ticks
            if (span.Ticks < (10_000 * 1000))
            {
                throw new ArgumentException("span.Ticks cannot be less than 1 second", nameof(span));
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
            if (IsCountdownRunning)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;

                timer = null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Created at: {0}", created.ToString(CultureInfo.CurrentCulture)));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Active: {0}", IsCountdownRunning));
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Time left: {0}", TimeLeft.ToString()));

            return sb.ToString();
        }
    }
}