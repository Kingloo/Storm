using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Storm
{
    public abstract class StreamBase : ViewModelBase
    {
        #region Fields
        protected string _apiUri = string.Empty;
        protected bool _hasUpdatedDisplayName = false;
        #endregion

        #region Properties
        protected Uri _uri = null;
        public Uri Uri
        {
            get { return this._uri; }
            set
            {
                this._uri = value;
                OnNotifyPropertyChanged();
            }
        }

        protected string _name = string.Empty;
        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                OnNotifyPropertyChanged();
            }
        }

        protected string _displayName = string.Empty;
        public string DisplayName
        {
            get { return this._displayName; }
            set
            {
                this._displayName = value;
                OnNotifyPropertyChanged();
            }
        }

        protected bool _isLive = false;
        public bool IsLive
        {
            get { return this._isLive; }
            set
            {
                this._isLive = value;
                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        protected string _game = "game unknown";
        public string Game
        {
            get { return this._game; }
            set
            {
                this._game = value;
                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        public string MouseOverTooltip
        {
            get
            {
                if (this.IsLive)
                {
                    return string.Format("{0} is live and playing {1}", this.DisplayName, this.Game);
                }
                else
                {
                    return string.Format("{0} is offline", this.DisplayName);
                }
            }
        }
        #endregion

        protected StreamBase(string streamerPageUri)
        {
            Uri tmp = null;
            if (Uri.TryCreate(streamerPageUri, UriKind.Absolute, out tmp))
            {
                this.Uri = tmp;
            }

            this.Name = SetAccountName(streamerPageUri);
            this.DisplayName = this.Name;
        }

        private string SetAccountName(string s)
        {
            return s.Substring(s.LastIndexOf("/") + 1);
        }

        protected async Task<JObject> GetApiResponseAsync(HttpWebRequest request)
        {
            string jsonResponse = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await request.GetResponseAsyncExt(2).ConfigureAwait(false)))
            {
                if (resp != null)
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            jsonResponse = await sr.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }
                }
            }


            JObject j = null;

            if (String.IsNullOrEmpty(jsonResponse) == false)
            {
                try
                {
                    j = JObject.Parse(jsonResponse);
                }
                catch (JsonReaderException jre)
                {
                    Utils.LogException(jre);
                }
            }

            return j;
        }

        protected void NotifyIsNowLive()
        {
            string title = string.Format("{0} is live", this.DisplayName);
            string description = string.Format("and playing {0}", this.Game);

            NotificationService.Send(title, description, this.Uri);
        }

        public abstract Task UpdateAsync();
        protected abstract Task<bool> TrySetDisplayNameAsync();
        protected abstract Task<bool> DetermineIfLive();
    }
}
