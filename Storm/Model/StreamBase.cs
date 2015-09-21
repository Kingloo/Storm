//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//namespace Storm.Model
//{
//    abstract class StreamBase : ViewModelBase
//    {
//        #region Fields
//        protected static Uri apiUri = null;
//        #endregion

//        #region Properties
//        protected Uri _accountUri = null;
//        public Uri AccountUri
//        {
//            get
//            {
//                return this._accountUri;
//            }
//            set
//            {
//                this._accountUri = value;

//                OnNotifyPropertyChanged();
//            }
//        }

//        protected string _name = string.Empty;
//        public string Name
//        {
//            get
//            {
//                return _name;
//            }
//            set
//            {
//                _name = value;

//                OnNotifyPropertyChanged();
//            }
//        }

//        protected string _displayName = string.Empty;
//        public string DisplayName
//        {
//            get
//            {
//                return _displayName;
//            }
//            set
//            {
//                _displayName = value;

//                OnNotifyPropertyChanged();
//            }
//        }

//        protected bool _isLive = false;
//        public bool IsLive
//        {
//            get
//            {
//                return this._isLive;
//            }
//            set
//            {
//                this._isLive = value;

//                OnNotifyPropertyChanged();
//            }
//        }

//        private bool _updating = false;
//        public bool Updating
//        {
//            get
//            {
//                return this._updating;
//            }
//            set
//            {
//                this._updating = value;

//                OnNotifyPropertyChanged();
//            }
//        }
//        #endregion

//        protected StreamBase(Uri accountUri)
//        {
//            _accountUri = accountUri;
//        }

//        public abstract static HttpWebRequest CreateHttpWebRequest(Uri uri);
//        public abstract static Task<JObject> GetApiResponseAsync(HttpWebRequest req);
//        public abstract static Task UpdateAllAsync(IEnumerable<StreamBase> streams);

//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder();

//            sb.AppendLine(this.GetType().ToString());
//            sb.AppendLine(string.Format("Account uri: {0}", AccountUri));
//            sb.AppendLine(string.Format("Name: {0}", Name));
//            sb.AppendLine(string.Format("Display name: {0}", DisplayName));
//            sb.AppendLine(string.Format("Is Live: {0}", IsLive.ToString()));

//            return sb.ToString();
//        }
//    }
//}
