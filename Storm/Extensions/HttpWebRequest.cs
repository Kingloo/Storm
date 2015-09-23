using System.Net;
using System.Threading.Tasks;

namespace Storm.Extensions
{
    public static class HttpWebRequestExt
    {
        public static WebResponse GetResponseExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

            try
            {
                webResp = req.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            return webResp;
        }

        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest req)
        {
            WebResponse webResp = null;

            try
            {
                webResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    webResp = e.Response;
                }
            }

            return webResp;
        }
    }
}
