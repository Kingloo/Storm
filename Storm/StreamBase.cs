using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm
{
    abstract class StreamBase : ViewModelBase
    {
        #region Commands
        private DelegateCommand _goToStreamCommand = null;
        public DelegateCommand GoToStreamCommand
        {
            get
            {
                if (this._goToStreamCommand == null)
                {
                    this._goToStreamCommand = new DelegateCommand(GoToStream, canExecute);
                }

                return this._goToStreamCommand;
            }
        }
        #endregion

        #region Fields
        protected Uri _uri = null;
        protected string _name = string.Empty;
        protected string _displayName = string.Empty;
        protected string _apiUri = string.Empty;
        protected bool _isLive = false;
        protected bool _hasUpdatedDisplayName = false;
        protected string _game = "game unknown";
        protected string _mouseOverTooltip = "user and game unknown";
        #endregion

        #region Properties
        public Uri Uri
        {
            get { return this._uri; }
            set
            {
                this._uri = value;
                OnPropertyChanged("Uri");
            }
        }
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
        public string Game
        {
            get { return this._game; }
            set
            {
                this._game = value;

                if (this.IsLive)
                {
                    this.MouseOverTooltip = string.Format("{0} is live and playing {1}", this.DisplayName, this.Game);
                }
                else
                {
                    this.MouseOverTooltip = string.Format("{0} is offline", this.DisplayName);
                }

                OnPropertyChanged("Game");
            }
        }
        public string MouseOverTooltip
        {
            get { return this._mouseOverTooltip; }
            set
            {
                this._mouseOverTooltip = value;
                OnPropertyChanged("MouseOverTooltip");
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

            if (wr != null)
            {
                wr.Close();
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

        protected void NotifyIsNowLive()
        {
            string title = string.Format("{0} is live", this.DisplayName);
            string description = string.Format("and playing {0}", this.Game);

            NotificationService.Send(title, description, this.Uri);
        }

        private void GoToStream(object parameter)
        {
            Misc.OpenUrlInBrowser(this._uri);
        }

        private bool canExecute(object parameter)
        {
            return true;
        }

        public abstract Task UpdateAsync();
        protected abstract Task<bool> TrySetDisplayNameAsync();
        protected abstract Task DetermineIfLive();
    }
}
