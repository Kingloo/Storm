using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected enum StreamingService { None, Twitch, Ustream, Justin, UnsupportedService };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
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
