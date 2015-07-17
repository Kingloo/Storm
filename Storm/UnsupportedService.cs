using System.Threading.Tasks;

namespace Storm
{
    class UnsupportedService : StreamBase
    {
        public UnsupportedService(string malFormatted)
            : base(string.Empty)
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

        protected override Task<bool> DetermineIfLive()
        {
            return null;
        }

        protected override void NotifyIsNowLive() { }
    }
}
