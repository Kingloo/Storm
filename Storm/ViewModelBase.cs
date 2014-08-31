using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    class ViewModelBase : INotifyPropertyChanged
    {
        protected enum StreamingService { None, Twitch, Ustream, Justin, UnsupportedService };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Dispatcher _dispatcher = Application.Current.Dispatcher;
        public Dispatcher Disp { get { return this._dispatcher; } }
    }
}
