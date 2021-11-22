using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace StormDesktop.Common
{
    public class DispatcherCountdownTimer
    {
        private readonly DateTime created = DateTime.Now;
        private readonly Action tick;
        private DispatcherTimer? timer = new DispatcherTimer(DispatcherPriority.Background);

        public bool IsRunning
        {
            get
            {
                return timer != null && timer.IsEnabled;
            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                return (timer is null)
                    ? TimeSpan.Zero
                    : (created + timer.Interval) - DateTime.Now;
            }
        }

        public DispatcherCountdownTimer(TimeSpan span, Action tick)
        {
            if (span.Ticks < (TimeSpan.TicksPerMillisecond * 1000))
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span.Ticks cannot be less than 1 second");
            }

            this.tick = tick ?? throw new ArgumentNullException(nameof(tick));

            timer.Interval = span;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            tick();

            Stop();
        }

        public void Start() => timer?.Start();

        public void Stop()
        {
            if (!(timer is null))
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
