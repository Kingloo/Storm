using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    class ViewModelBase : INotifyPropertyChanged
    {
        protected enum StreamingService { None, Twitch, Ustream, Justin, UnsupportedService };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(name));
            }
        }

        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        public Dispatcher Disp { get { return this._dispatcher; } }
    }
}
