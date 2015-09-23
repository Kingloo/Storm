using System.Threading.Tasks;

namespace Storm
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
            return null;
        }

        protected override Task DetermineIfLiveAsync()
        {
            return new Task(null);
        }

        protected override void NotifyIsNowLive() { }
    }
}
