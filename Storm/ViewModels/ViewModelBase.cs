using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Storm.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = default(string))
        {
            PropertyChangedEventHandler pceh = PropertyChanged;

            if ((pceh != null) && (String.IsNullOrWhiteSpace(propertyName) == false))
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);

                pceh(this, args);
            }
        }
    }
}
