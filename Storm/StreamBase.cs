using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storm.ViewModels;

namespace Storm
{
    public abstract class StreamBase : ViewModelBase
    {
        #region Fields
        protected string apiUri = string.Empty;
        protected bool hasUpdatedDisplayName = false;
        #endregion

        #region Properties
        protected Uri _uri = null;
        public Uri Uri
        {
            get
            {
                return this._uri;
            }
            set
            {
                this._uri = value;

                OnNotifyPropertyChanged();
            }
        }

        protected string _name = string.Empty;
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;

                OnNotifyPropertyChanged();
            }
        }

        protected string _displayName = string.Empty;
        public string DisplayName
        {
            get
            {
                return this._displayName;
            }
            set
            {
                this._displayName = value;

                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        protected bool _isLive = false;
        public bool IsLive
        {
            get
            {
                return this._isLive;
            }
            set
            {
                this._isLive = value;

                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        public abstract string MouseOverTooltip { get; }

        private bool _updating = false;
        public bool Updating
        {
            get
            {
                return this._updating;
            }
            set
            {
                this._updating = value;

                OnNotifyPropertyChanged();
            }
        }

        protected bool _isValid = false;
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }
        #endregion

        protected StreamBase(Uri accountUri)
        {
            if (accountUri != null)
            {
                this.Uri = accountUri;
                this.Name = SetAccountName(accountUri.AbsoluteUri);
                this.DisplayName = this.Name;
            }
        }

        private string SetAccountName(string s)
        {
            return s.Substring(s.LastIndexOf("/") + 1);
        }

        protected async Task<JObject> GetApiResponseAsync(HttpWebRequest req)
        {
            string jsonResponse = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await req.GetResponseAsyncExt().ConfigureAwait(false)))
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
                    else
                    {
                        string errorMessage = string.Format("Request for {0} failed: {1}", req.RequestUri.AbsoluteUri, resp.StatusCode);

                        Utils.LogMessage(errorMessage);
                    }
                }
                else
                {
                    if (req != null)
                    {
                        req.Abort();
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

        public abstract Task UpdateAsync();
        protected abstract Task DetermineIfLiveAsync();
        protected abstract void NotifyIsNowLive();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.GetType().ToString());
            sb.AppendLine(string.Format("Uri: {0}", Uri));
            sb.AppendLine(string.Format("Name: {0}", Name));
            sb.AppendLine(string.Format("Display name: {0}", DisplayName));
            sb.AppendLine(string.Format("Is Live: {0}", IsLive.ToString()));
             sb.AppendLine(string.Format("MouseOverToolTip: {0}", MouseOverTooltip));

            return sb.ToString();
        }
    }
}
