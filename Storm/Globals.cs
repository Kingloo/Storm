using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public static class Globals
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:40.0) Gecko/20100101 Firefox/41.0";

        public readonly static string UrlsFilePath = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
    }
}
