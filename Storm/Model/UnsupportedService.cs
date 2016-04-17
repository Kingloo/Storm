using System.Threading.Tasks;

namespace Storm.Model
{
    class UnsupportedService : StreamBase
    {
        public UnsupportedService(string malFormatted)
            : base(null)
        {
            this.Name = malFormatted;
            this.DisplayName = malFormatted;
        }

        public override string MouseOverTooltip
        {
            get
            {
                return string.Format("{0} is malformatted", this.Name);
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
