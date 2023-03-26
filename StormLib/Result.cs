using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace StormLib
{
	public class Result
	{
		public IList<HttpStatusCode> Statuses { get; init; } = Array.Empty<HttpStatusCode>();

		public Result() { }

		public Result(HttpStatusCode statusCode)
		{
			Statuses = new [] { statusCode };
		}

		public Result(IEnumerable<HttpStatusCode> statusCodes)
		{
			Statuses = statusCodes.ToArray();
		}
	}
}
