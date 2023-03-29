using System.Diagnostics.CodeAnalysis;

namespace StormLib.Helpers
{
	public static class JsonHelpers
	{
/*
		public static bool TryParse(string text, [NotNullWhen(true)] out Newtonsoft.Json.Linq.JObject? json)
		{
			try
			{
				json = Newtonsoft.Json.Linq.JObject.Parse(text);

				return true;
			}
			catch (Newtonsoft.Json.JsonReaderException)
			{
				json = null;

				return false;
			}
		}
*/

		public static bool TryParse(string text, [NotNullWhen(true)] out System.Text.Json.Nodes.JsonNode? json)
		{
			try
			{
				json = System.Text.Json.Nodes.JsonNode.Parse(text);

				return json is not null;
			}
			catch (System.Text.Json.JsonException)
			{
				json = null;

				return false;
			}
		}
	}
}
