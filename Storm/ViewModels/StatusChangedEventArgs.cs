using System;

namespace Storm.ViewModels
{
    public class StatusChangedEventArgs : EventArgs
    {
        private readonly bool _isUpdating = false;
        public bool IsUpdating => _isUpdating;

        public StatusChangedEventArgs(bool isUpdating)
        {
            _isUpdating = isUpdating;
        }
    }
}
