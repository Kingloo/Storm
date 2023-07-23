using System;

namespace StormLib.Common
{
	// https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html

	public static class ExceptionFilterUtility
	{
		public static bool True(Action action)
		{
			ArgumentNullException.ThrowIfNull(action, nameof(action));

			action();

			return true;
		}

		public static bool False(Action action)
		{
			ArgumentNullException.ThrowIfNull(action, nameof(action));

			action();

			return false;
		}
	}
}
