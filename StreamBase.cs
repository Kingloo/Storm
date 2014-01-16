using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System;

namespace Storm
{
    abstract class StreamBase : ViewModelBase
    {
        protected string _uri = string.Empty;
        protected string _name = string.Empty;
        protected string _displayName = string.Empty;
        protected string _apiUri = string.Empty;
        protected bool _isLive = false;

        public string Uri { get { return this._uri; } }
        public string Name { get { return this._name; } }

        public string DisplayName
        {
            get { return this._displayName; }
            set
            {
                this._displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public bool IsLive
        {
            get { return this._isLive; }
            set
            {
                this._isLive = value;
                OnPropertyChanged("IsLive");
            }
        }

        protected StreamBase(string streamerPageUri)
        {
            this._uri = streamerPageUri;
            this._name = GetAccountName(streamerPageUri);
            this._displayName = this._name;

            this.HasGoneLive += StreamBase_HasGoneLive;
        }

        private void StreamBase_HasGoneLive(object sender, StreamBase e)
        {
            string message = string.Format("{0} is now live!", this.DisplayName);

            this.notificationManager.CreateNotification(message, new TimeSpan(0, 0, 15), this.Uri);
        }

        private string GetAccountName(string s)
        {
            return s.Substring(s.LastIndexOf("/") + 1);
        }

        protected JObject GetApiResponse(string fullApiRequestAddress)
        {
            HttpWebRequest httpWebReq = HttpWebRequest.CreateHttp(fullApiRequestAddress);
            httpWebReq.Accept = ("application/vnd.twitchtv.v2+json");
            httpWebReq.KeepAlive = false;
            httpWebReq.Method = "GET";
            httpWebReq.Referer = "twitch.tv";
            httpWebReq.Timeout = 2500;
            httpWebReq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            
            string jsonResponse = string.Empty;
            WebResponse webResp = null;

            try
            {
                webResp = httpWebReq.GetResponse();
            }
            catch (ProtocolViolationException)
            {
                return null;
            }
            catch (WebException)
            {
                return null;
            }

            using (StreamReader sr = new StreamReader(webResp.GetResponseStream()))
            {
                jsonResponse = sr.ReadToEnd();
            }

            if (jsonResponse != string.Empty)
            {
                JObject j = JObject.Parse(jsonResponse);

                if (j != null)
                {
                    return j;
                }
            }

            return null;
        }

        public abstract void Update();
        protected abstract void ProcessApiResponse(JObject jobj);
        protected abstract string SetDisplayName();
    }
}
