//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Cache;
//using System.Text;
//using System.Threading.Tasks;
//using System.Linq;
//using Newtonsoft.Json.Linq;

//namespace Storm.Model
//{
//    class Twitch : StreamBase
//    {
//        #region Properties
//        private string _game = string.Empty;
//        public string Game
//        {
//            get
//            {
//                return _game;
//            }
//            set
//            {
//                _game = value;

//                OnNotifyPropertyChanged();
//            }
//        }
//        #endregion

//        public Twitch(Uri accountUri)
//            : base(accountUri)
//        {
//            apiUri = new Uri("https://api.twitch.tv/kraken");
//        }

//        public static override async Task UpdateAllAsync(IEnumerable<StreamBase> streams)
//        {
//            foreach (Twitch each in streams)
//            {
//                each.Updating = true;
//            }
            
//            StringBuilder sb = new StringBuilder();

//            foreach (Twitch each in streams)
//            {
//                sb.Append(string.Format("{0},", each.Name));
//            }

//            string uri = string.Format("{0}/streams?channel={1}", apiUri, sb.ToString());

//            HttpWebRequest req = CreateHttpWebRequest(new Uri(uri));

//            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

//            if (resp != null)
//            {
//                if (resp["streams"].HasValues)
//                {
//                    foreach (JToken each in resp["streams"])
//                    {
//                        if (each.HasValues)
//                        {
//                            StreamBase t = (from twitch in streams
//                                            where twitch.Name.Equals((string)each["channel"]["name"])
//                                            select twitch).First<StreamBase>();
//                        }
//                    }
//                }
//            }

//            foreach (Twitch each in streams)
//            {
//                each.Updating = false;
//            }
//        }

//        public override static HttpWebRequest CreateHttpWebRequest(Uri uri)
//        {
//            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

//            req.Accept = "application/vnd.twitchtv.v3+json";
//            req.AllowAutoRedirect = true;
//            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
//            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
//            req.Host = uri.DnsSafeHost;
//            req.KeepAlive = false;
//            req.Method = "GET";
//            req.ProtocolVersion = HttpVersion.Version11;
//            req.Referer = string.Format("{0}://{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
//            req.Timeout = 4000;
//            req.UserAgent = Globals.UserAgent;

//            req.Headers.Add("DNT", "1");
//            req.Headers.Add("Accept-Encoding", "gzip, deflate");

//            return req;
//        }

//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder();

//            sb.Append(base.ToString());
//            sb.AppendLine(Game);

//            return sb.ToString();
//        }
//    }
//}
