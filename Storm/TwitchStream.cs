using Newtonsoft.Json.Linq;

namespace Storm
{
    class TwitchStream : StreamBase
    {
        private bool isFirstUpdate = true;

        public TwitchStream(string s)
            : base(s)
        {
            this._apiUri = "https://api.twitch.tv/kraken";
        }

        protected override string SetDisplayName()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", this._apiUri, this._name);
            JObject response = GetApiResponse(apiAddressToQueryForDisplayName);

            string displayName = this._name;

            if (response != null)
            {
                if (response["display_name"] != null)
                {
                    displayName = (string)response["display_name"];
                }
            }

            return displayName;
        }

        public override void Update()
        {
            if (this.isFirstUpdate)
            {
                this.DisplayName = SetDisplayName();
                this.isFirstUpdate = false;
            }

            string streamApiAddress = string.Format("{0}/streams/{1}", this._apiUri, this._name);

            JObject apiResponse = GetApiResponse(streamApiAddress);

            if (apiResponse != null)
            {
                ProcessApiResponse(apiResponse);
            }
        }

        protected override void ProcessApiResponse(JObject jobj)
        {
            if (jobj["stream"] is JToken)
            {
                if (jobj["stream"].HasValues)
                {
                    if (this._isLive == false)
                    {
                        this.IsLive = true;

                        this.OnHasGoneLive(this);
                    }
                }
                else
                {
                    if (this._isLive == true)
                    {
                        this.IsLive = false;
                    }
                }
            }
        }
    }
}
