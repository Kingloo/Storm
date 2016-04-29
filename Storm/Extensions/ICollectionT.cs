using System;
using System.Collections.Generic;
using System.Linq;

namespace Storm.Extensions
{
    public static class ICollectionTExtensions
    {
        public static void AddList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (list == null) throw new ArgumentNullException(nameof(list));

            foreach (T each in list)
            {
                collection.Add(each);
            }
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (list == null) throw new ArgumentNullException(nameof(list));

            int countAdded = 0;

            foreach (T each in list)
            {
                if (collection.Contains(each) == false)
                {
                    collection.Add(each);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static int AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            int countAdded = 0;

            foreach (T each in list)
            {
                if (collection.Contains<T>(each, comparer) == false)
                {
                    collection.Add(each);

                    countAdded++;
                }
            }

            return countAdded;
        }

        public static void RemoveList<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (list == null) throw new ArgumentNullException(nameof(list));

            foreach (T obj in list)
            {
                collection.Remove(obj);
            }
        }
		
		public static void AlternativeSort<T>(this ICollection<T> collection, T mustSortFirst, T mustSortLast) where T : IComparable<T>, IAlternativeSort
		{
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            List<T> all = new List<T>(collection);
			
			all.Sort();
			
			foreach (T each in collection)
			{
				if (each.SortId != Int32.MinValue || each.SortId != Int32.MaxValue)
				{
					each.SortId = all.IndexOf(each) + 1;
				}
			}
			
			if (mustSortFirst != null) mustSortFirst.SortId = Int32.MinValue;
			if (mustSortLast != null) mustSortLast.SortId = Int32.MaxValue;
		}
    }
	
	public interface IAlternativeSort
	{
		int SortId { get; set; }
	}
}
