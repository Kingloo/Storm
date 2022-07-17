using System;
using System.Globalization;
using System.Text;
using System.Windows.Threading;

namespace StormDesktop.Common
{
	public class DispatcherCountdownTimer
	{
		private readonly TimeSpan span;
		private readonly Action tick;

		private readonly DateTime created = DateTime.Now;
		private DispatcherTimer? timer = null;

		public bool IsRunning => timer?.IsEnabled ?? false;

		public TimeSpan TimeLeft => IsRunning
			? ((created + (timer?.Interval ?? TimeSpan.Zero)) - DateTime.Now)
			: TimeSpan.Zero;

		public DispatcherCountdownTimer(TimeSpan span, Action tick)
		{
			if (span.Ticks < (10_000 * 1000))
			{
				// there are 10_000 ticks in 1 millisecond
				// therefore there are 10_000 * 1000 ticks in 1 second
				// 10_000 * 1000 = 10_000_000

				throw new ArgumentOutOfRangeException(nameof(span), "span.Ticks cannot be less than 1 second");
			}

			if (tick is null)
			{
				throw new ArgumentNullException(nameof(tick));
			}

			this.tick = tick;
			this.span = span;
		}

		public void Start()
		{
			timer = new DispatcherTimer(DispatcherPriority.Background);
			timer.Interval = span;
			timer.Tick += Timer_Tick;

			timer.Start();
		}

		private void Timer_Tick(object? sender, EventArgs e)
		{
			tick();

			Stop();
		}

		public void Stop()
		{
			if (timer is not null)
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
