using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storm.ViewModels;
using Storm.Extensions;
using System.Diagnostics;

namespace Storm.Model
{
    public abstract class StreamBase : ViewModelBase
    {
        #region Fields
        private string _apiUri = string.Empty;
        protected string ApiUri
        {
            get
            {
                return _apiUri;
            }
            set
            {
                _apiUri = value;
            }
        }

        private bool _hasUpdatedDisplayName = false;
        protected bool HasUpdatedDisplayName
        {
            get
            {
                return _hasUpdatedDisplayName;
            }
            set
            {
                _hasUpdatedDisplayName = value;
            }
        }
        #endregion

        #region Properties
        private Uri _uri = null;
        public Uri Uri
        {
            get
            {
                return _uri;
            }
            protected set
            {
                _uri = value;

                OnNotifyPropertyChanged();
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get
            {
                return _name;
            }
            protected set
            {
                _name = value;

                OnNotifyPropertyChanged();
            }
        }

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            protected set
            {
                _displayName = value;

                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get
            {
                return _isLive;
            }
            protected set
            {
                _isLive = value;

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
                return _updating;
            }
            set
            {
                _updating = value;

                OnNotifyPropertyChanged();
            }
        }

        private bool _isValid = false;
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            protected set
            {
                _isValid = value;
            }
        }
        #endregion

        protected StreamBase(Uri accountUri)
        {
            if (accountUri == null) throw new ArgumentNullException(nameof(accountUri), "StreamBase ctor was passed a null uri");

            Uri = accountUri;
            Name = SetAccountName(accountUri.AbsoluteUri);
            DisplayName = Name;
        }

        private static string SetAccountName(string text)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }

            return text.Substring(text.LastIndexOf("/") + 1);
        }

        protected async Task<JObject> GetApiResponseAsync(HttpWebRequest request)
        {
            string jsonResponse = string.Empty;

            using (HttpWebResponse resp = (HttpWebResponse)(await request.GetResponseAsyncExt().ConfigureAwait(false)))
            {
                if (resp == null)
                {
                    request?.Abort();
                }
                else
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
                catch (JsonReaderException e)
                {
                    Utils.LogException(e);
                }
            }

            return j;
        }

        public abstract Task UpdateAsync();
        protected abstract Task DetermineIfLiveAsync();
        protected abstract void NotifyIsNowLive();

        public virtual void GoToStream()
        {
            string args = string.Format(@"/C livestreamer.exe {0} best", Uri.AbsoluteUri);

            ProcessStartInfo pInfo = new ProcessStartInfo
            {
                Arguments = args,
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(pInfo);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().ToString());
            sb.AppendLine(string.Format("Uri: {0}", Uri));
            sb.AppendLine(string.Format("Name: {0}", Name));
            sb.AppendLine(string.Format("Display name: {0}", DisplayName));
            sb.AppendLine(string.Format("Is Live: {0}", IsLive.ToString()));
            sb.AppendLine(string.Format("MouseOverToolTip: {0}", MouseOverTooltip));

            return sb.ToString();
        }
    }
}
