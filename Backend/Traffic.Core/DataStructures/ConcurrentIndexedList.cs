using System.Collections;

namespace Traffic.Core.DataStructures
{
    public class ConcurrentIndexedList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = [];
        private readonly ReaderWriterLockSlim _lock = new();

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _list.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _list[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _list.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                if (index < 0 || index >= _list.Count)
                    return false;

                _list.RemoveAt(index);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<T> GetAll()
        {
            _lock.EnterReadLock();
            try
            {
                return [.. _list];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _list.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<T> copy;
            _lock.EnterReadLock();
            try
            {
                copy = [.. _list];
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}