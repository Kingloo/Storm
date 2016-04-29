using System;
using System.Net;
using System.Threading.Tasks;

namespace Storm.Extensions
{
    public static class HttpWebRequestExt
    {
        public static WebResponse GetResponseExt(this HttpWebRequest request)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

            WebResponse webResp = null;

            try
            {
                webResp = request.GetResponse();
            }
            catch (WebException e)
            {
                webResp = e?.Response;
            }

            return webResp;
        }

        public static async Task<WebResponse> GetResponseAsyncExt(this HttpWebRequest request)
        {
            if (request == null) { throw new ArgumentNullException(nameof(request)); }

            WebResponse webResp = null;

            try
            {
                webResp = await request.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                webResp = e?.Response;
            }

            return webResp;
        }
    }
}
