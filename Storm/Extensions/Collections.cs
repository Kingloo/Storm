using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Storm.Extensions
{
    public static class CollectionExt
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            
            foreach (T each in list)
            {
                collection.Add(each);
            }
        }


        public static void AddSorted<T>(this Collection<T> collection, T item) where T : IComparable<T>
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (item == null) { throw new ArgumentNullException(nameof(item)); }

            int i = 0;

            while (i < collection.Count
                && collection[i].CompareTo(item) < 0)
            {
                i++;
            }

            collection.Insert(i, item);
        }

        public static void AddSorted<T>(this Collection<T> collection, T item, IComparer<T> comparer)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (item == null) { throw new ArgumentNullException(nameof(item)); }
            if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

            int i = 0;

            while (i < collection.Count
                && comparer.Compare(collection[i], item) < 0)
            {
                i++;
            }

            collection.Insert(i, item);
        }


        public static void AddRangeSorted<T>(this Collection<T> collection, IEnumerable<T> list) where T : IComparable<T>
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            foreach (T each in list)
            {
                collection.AddSorted(each);
            }
        }

        public static void AddRangeSorted<T>(this Collection<T> collection, IEnumerable<T> list, IComparer<T> comparer)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

            foreach (T each in list)
            {
                collection.AddSorted(each, comparer);
            }
        }


        public static void AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list) where T : IEquatable<T>
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            foreach (T each in list)
            {
                if (collection.Contains(each) == false)
                {
                    collection.Add(each);
                }
            }
        }

        public static void AddMissing<T>(this ICollection<T> collection, IEnumerable<T> list, IEqualityComparer<T> comparer)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

            foreach (T each in list)
            {
                if (collection.Contains(each, comparer) == false)
                {
                    collection.Add(each);
                }
            }
        }


        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> list)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            foreach (T each in list)
            {
                if (collection.Contains(each))
                {
                    collection.Remove(each);
                }
            }
        }
    }
}