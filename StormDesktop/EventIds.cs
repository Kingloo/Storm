using Microsoft.Extensions.Logging;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace StormDesktop.EventIds
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
	internal static class StormBackgroundService
	{
		internal const int StartingId = 101;
		internal const int StartedId = 102;
		internal const int UpdateStartedId = 111;
		internal const int UpdateEndedId = 112;
		internal const int DelayStartedId = 113;
		internal const int DelayEndedId = 114;
		internal const int StoppedCancelledId = 191;
		internal const int StoppedNotCancelledId = 192;
		internal const int StoppedUnexpectedlyId = 193;
		internal const int StoppingId = 199;

		internal static readonly EventId Starting = new EventId(StartingId, nameof(Starting));
		internal static readonly EventId Started = new EventId(StartedId, nameof(Started));
		internal static readonly EventId UpdateStarted = new EventId(UpdateStartedId, nameof(UpdateStarted));
		internal static readonly EventId UpdateEnded = new EventId(UpdateEndedId, nameof(UpdateEnded));
		internal static readonly EventId DelayStarted = new EventId(DelayStartedId, nameof(DelayStarted));
		internal static readonly EventId DelayEnded = new EventId(DelayEndedId, nameof(DelayEnded));
		internal static readonly EventId StoppedCancelled = new EventId(StoppedCancelledId, nameof(StoppedCancelled));
		internal static readonly EventId StoppedNotCancelled = new EventId(StoppedNotCancelledId, nameof(StoppedNotCancelled));
		internal static readonly EventId StoppedUnexpectedly = new EventId(StoppedUnexpectedlyId, nameof(StoppedUnexpectedly));
		internal static readonly EventId Stopping = new EventId(StoppingId, nameof(Stopping));
	}
}