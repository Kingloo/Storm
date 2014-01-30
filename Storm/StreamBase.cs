using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm
{
    abstract class StreamBase : ViewModelBase
    {
        protected string _uri = string.Empty;
        protected string _name = string.Empty;
        protected string _displayName = string.Empty;
        protected string _apiUri = string.Empty;
        protected bool _isLive = false;
        protected bool _hasUpdatedDisplayName = false;

        public string Uri { get { return this._uri; } }
        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                OnPropertyChanged("Name");
            }
        }
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
            this.Name = SetAccountName(streamerPageUri);
            this.DisplayName = this.Name;

            this.HasGoneLive += StreamBase_HasGoneLive;
        }

        protected string SetAccountName(string s)
        {
            return s.Substring(s.LastIndexOf("/") + 1);
        }

        private void StreamBase_HasGoneLive(object sender, StreamBase e)
        {
            string message = string.Format("{0} is now live!", this.DisplayName);

            Disp.Invoke(new Action(
                delegate()
                {
                    this.notificationManager.CreateNotification(message, new TimeSpan(0, 0, 15), this.Uri);
                }), System.Windows.Threading.DispatcherPriority.SystemIdle);
        }

        protected async Task<JObject> GetApiResponseAsync(HttpWebRequest request)
        {
            string jsonResponse = string.Empty;

            WebResponse wr = null;

            try
            {
                wr = await request.GetResponseAsync();

                using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                {
                    jsonResponse = await sr.ReadToEndAsync();
                }
            }
            catch (WebException)
            {
                jsonResponse = string.Empty;
            }
            finally
            {
                if (wr != null)
                {
                    wr.Close();
                }
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

        public abstract Task UpdateAsync();
        protected abstract Task<bool> TrySetDisplayNameAsync();
        protected abstract void ProcessApiResponse(JObject jobj);
    }
}
