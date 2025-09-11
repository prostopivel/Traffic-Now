using System.Collections;

namespace Traffic.Core.DataStructures
{
    public class ConcurrentIndexedSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _set = [];
        private readonly List<T> _indexedList = [];
        private readonly ReaderWriterLockSlim _lock = new();

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _set.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_set.Add(item))
                {
                    _indexedList.Add(item);
                    return true;
                }
                return false;
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
                    return _indexedList[index];
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
                if (_set.Remove(item))
                {
                    _indexedList.Remove(item);
                    return true;
                }
                return false;
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
                if (index < 0 || index >= _indexedList.Count)
                    return false;

                var item = _indexedList[index];
                _set.Remove(item);
                _indexedList.RemoveAt(index);
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
                return [.. _indexedList];
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
                return _set.Contains(item);
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
                _set.Clear();
                _indexedList.Clear();
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
                copy = [.. _indexedList];
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