using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Storm.Wpf.Common;
using Storm.Wpf.StreamServices;

namespace Storm.Wpf.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields
        private readonly FileLoader fileLoader = null;
        #endregion

        #region Properties
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, nameof(IsActive));
        }

        private readonly ObservableCollection<IStream> _streams = new ObservableCollection<IStream>();
        public IReadOnlyCollection<IStream> Streams => _streams;
        #endregion

        #region Commands
        private DelegateCommand<Window> _exitCommand = null;
        public DelegateCommand<Window> ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand<Window>(window => window.Close(), _ => true);
                }

                return _exitCommand;
            }
        }
        #endregion

        public MainWindowViewModel(FileLoader fileLoader)
        {
            this.fileLoader = fileLoader ?? throw new ArgumentNullException(nameof(fileLoader));
        }
    }
}
