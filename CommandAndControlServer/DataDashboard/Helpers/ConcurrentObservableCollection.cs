using System.Collections.ObjectModel;

namespace DataDashboard.Helpers
{
    public class ConcurrentObservableCollection<T> : ObservableCollection<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        // Hide base indexer for thread safety
        new public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return base[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
        public ConcurrentObservableCollection() : base() { }
        public ConcurrentObservableCollection(IEnumerable<T> collection) : base(collection) { }
        public ConcurrentObservableCollection(List<T> list) : base(list) { }
        
        new public virtual void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                base.Add(item);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        new public virtual bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return base.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        new public void Move(int oldIndex, int newIndex)
        {
            _lock.EnterWriteLock();
            try
            {
                base.Move(oldIndex, newIndex);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
