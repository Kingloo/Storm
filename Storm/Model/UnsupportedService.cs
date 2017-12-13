using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Storm.Common;

namespace Storm.Model
{
    public class UnsupportedService : StreamBase
    {
        #region Fields
        // to avoid writing an error message on every update
        private bool messageWritten = false;
        #endregion

        #region Properties
        public override string MouseOverTooltip
            => string.Format(CultureInfo.CurrentCulture, "{0} is an unsupported service", Name);

        private static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/UnsupportedService.ico"));
        public override BitmapImage Icon => _icon;
        #endregion

        public UnsupportedService(string malFormatted)
            : base(new Uri(malFormatted))
        {
            Name = malFormatted;
        }

        public override async Task UpdateAsync()
            => await WriteMessage();

        protected override async Task DetermineIfLiveAsync()
            => await WriteMessage();

        private async Task WriteMessage()
        {
            if (!messageWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "{0} is an unsupported service.", Name);

                await Log.LogMessageAsync(message).ConfigureAwait(false);

                messageWritten = true;
            }
        }
    }
}
