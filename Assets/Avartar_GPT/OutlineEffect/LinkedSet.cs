using System.Collections;
using System.Collections.Generic;

namespace cakeslice
{
    public class LinkedSet<T> : IEnumerable<T>
    {
        public enum AddType
        {
	        /// <summary>
	        ///     No changes were made
	        /// </summary>
	        NO_CHANGE,

	        /// <summary>
	        ///     The value was added
	        /// </summary>
	        ADDED,

	        /// <summary>
	        ///     The value was moved to the end.
	        /// </summary>
	        MOVED
        }

        private readonly Dictionary<T, LinkedListNode<T>> dictionary;

        private readonly LinkedList<T> list;

        public LinkedSet()
        {
            list = new LinkedList<T>();
            dictionary = new Dictionary<T, LinkedListNode<T>>();
        }

        public LinkedSet(IEqualityComparer<T> comparer)
        {
            list = new LinkedList<T>();
            dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool Contains(T t)
        {
            return dictionary.ContainsKey(t);
        }

        public bool Add(T t)
        {
            if (dictionary.ContainsKey(t)) return false;

            var node = list.AddLast(t);
            dictionary.Add(t, node);
            return true;
        }

        public void Clear()
        {
            list.Clear();
            dictionary.Clear();
        }

        public AddType AddOrMoveToEnd(T t)
        {
            LinkedListNode<T> node;

            if (dictionary.Comparer.Equals(t, list.Last.Value)) return AddType.NO_CHANGE;

            if (dictionary.TryGetValue(t, out node))
            {
                list.Remove(node);
                node = list.AddLast(t);
                dictionary[t] = node;
                return AddType.MOVED;
            }

            node = list.AddLast(t);
            dictionary[t] = node;
            return AddType.ADDED;
        }

        public bool Remove(T t)
        {
            LinkedListNode<T> node;

            if (dictionary.TryGetValue(t, out node) && dictionary.Remove(t))
            {
                list.Remove(node);
                return true;
            }

            return false;
        }

        public void ExceptWith(IEnumerable<T> enumerable)
        {
            foreach (var t in enumerable) Remove(t);
        }
    }
}