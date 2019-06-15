using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Storm.Wpf.Common
{
    public static class Json
    {
        public static bool TryParse(string input, out JObject json)
        {
            try
            {
                json = JObject.Parse(input);

		        return true;
            }
            catch (JsonReaderException)
            {
                json = null;

		        return false;
            }
        }
    }
}
