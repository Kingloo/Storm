using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} is an unsupported service", Name);
            }
        }

        public override BitmapImage Icon
        {
            get
            {
                return null;
            }
        }
        #endregion

        public UnsupportedService(string malFormatted)
            : base(new Uri(malFormatted))
        {
            Name = malFormatted;
        }

        public override async Task UpdateAsync()
        {
            await WriteMessage();
        }

        protected override async Task DetermineIfLiveAsync()
        {
            await WriteMessage();
        }

        private async Task WriteMessage()
        {
            if (!messageWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "{0} is an unsupported service.", Name);

                await Utils.LogMessageAsync(message);

                messageWritten = true;
            }
        }
    }
}
