using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Storm.Wpf.Streams;

namespace Storm.Wpf.StreamServices
{
    public abstract class StreamServiceBase
    {
        protected abstract Uri ApiRoot { get; }
        public abstract Type HandlesStreamType { get; }


        public virtual ProcessStartInfo GetWatchingInstructions(StreamBase stream)
        {
            string command = $"/C streamlink {stream.AccountLink} best";

            return new ProcessStartInfo
            {
                Arguments = command,
                FileName = "powershell.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true // maybe remove
            };
        }

        public virtual ProcessStartInfo GetRecordingInstructions(StreamBase stream)
        {
            string command = $"/C youtube-dl {stream.AccountLink}";

            return new ProcessStartInfo
            {
                Arguments = command,
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true // maybe remove
            };
        }


        public abstract Task UpdateAsync(IEnumerable<StreamBase> streams);


        public override bool Equals(object obj)
        {
            if (obj is StreamServiceBase service)
            {
                return HandlesStreamType == service.HandlesStreamType;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => HandlesStreamType.GetHashCode();
    }
}
