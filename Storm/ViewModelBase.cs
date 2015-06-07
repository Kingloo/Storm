using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Storm
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected enum StreamingService
        {
            None,
            Twitch,
            Ustream,
            Justin,
            UnsupportedService
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = default(string))
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;

            if ((pceh != null) && (String.IsNullOrWhiteSpace(propertyName) == false))
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);

                pceh(this, args);
            }
        }

    }
}
