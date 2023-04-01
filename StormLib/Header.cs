namespace StormLib
{
	public class Header
	{
		public string Name { get; init; } = string.Empty;
		public string Value { get; init; } = string.Empty;

		public Header() { }

		public Header(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}