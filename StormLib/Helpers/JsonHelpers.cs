using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StormLib.Helpers
{
	public static class JsonHelpers
	{
		public static bool TryParse(string text, out JObject? json)
		{
			try
			{
				json = JObject.Parse(text);

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
