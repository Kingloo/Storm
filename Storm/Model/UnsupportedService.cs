using System.Globalization;
using System.Threading.Tasks;

namespace Storm.Model
{
    public class UnsupportedService : StreamBase
    {
        public override string MouseOverTooltip
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} is malformatted", Name);
            }
        }

        public UnsupportedService(string malFormatted)
            : base(null)
        {
            Name = malFormatted;
            DisplayName = malFormatted;

            // only UnsupportedService should set this to false
            IsValid = false;
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
