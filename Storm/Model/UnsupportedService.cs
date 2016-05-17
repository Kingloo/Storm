using System.Globalization;
using System.Threading.Tasks;

namespace Storm.Model
{
    class UnsupportedService : StreamBase
    {
        public UnsupportedService(string malFormatted)
            : base(null)
        {
            Name = malFormatted;
            DisplayName = malFormatted;
        }

        public override string MouseOverTooltip
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} is malformatted", Name);
            }
        }

        public override Task UpdateAsync()
        {
            return new Task(() => { return; });
        }

        protected override Task DetermineIfLiveAsync()
        {
            return new Task(() => { return; });
        }

        protected override void NotifyIsNowLive() { }
    }
}
