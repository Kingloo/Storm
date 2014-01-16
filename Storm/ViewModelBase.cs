using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    class ViewModelBase : INotifyPropertyChanged
    {
        protected enum StreamingService { None, Twitch, Ustream, Justin, UnsupportedService };
        protected NotificationManager notificationManager = new NotificationManager();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(name));
            }
        }

        public event EventHandler<StreamBase> HasGoneLive;
        protected void OnHasGoneLive(StreamBase sb)
        {
            EventHandler<StreamBase> handler = this.HasGoneLive;
            if (handler != null)
            {
                handler(this, sb);
            }
        }

        private Application _app = Application.Current;
        public Application App { get { return this._app; } }

        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        public Dispatcher Disp { get { return this._dispatcher; } }
    }
}
