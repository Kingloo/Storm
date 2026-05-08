using System;
using System.Collections.Generic;

namespace StormLib.Common
{
	internal sealed class EmptyList<T> : List<T>
	{
		private static readonly EmptyList<T> emptyList = new EmptyList<T>();

		internal EmptyList()
		{
			Capacity = 0;
		}

#pragma warning disable CA1822 // Mark members as static
		internal new void Add(T t)
#pragma warning restore CA1822 // Mark members as static
		{
			throw new InvalidOperationException("cannot .Add to EmptyList<T>");
		}

		internal static EmptyList<T> Empty { get => emptyList; }
	}
}
