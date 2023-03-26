using System.Net;
using StormLib.Services;

namespace StormLib
{
	public class Result
	{
		public UpdaterType UpdaterType { get; } = UpdaterType.None;
		public HttpStatusCode StatusCode { get; } = HttpStatusCode.Unused;
		public string Message { get; init; } = string.Empty;

		public Result(UpdaterType updaterType)
		{
			UpdaterType = updaterType;
		}

		public Result(UpdaterType updaterType, HttpStatusCode statusCode)
			: this(updaterType)
		{
			StatusCode = statusCode;
		}
	}
}
