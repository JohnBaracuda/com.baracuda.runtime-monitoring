using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Experimental
{
    public class MonitoredList<T> : MonitoredType, IList<T>
    {
        #region --- List ---

        private readonly List<T> _list;

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            SetDirty();
        }

        public void Clear()
        {
            _list.Clear();
            SetDirty();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
            SetDirty();
        }

        public bool Remove(T item)
        {
            var result = _list.Remove(item);
            SetDirty();
            return result;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
        
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            SetDirty();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            SetDirty();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _list[index] = value;
                SetDirty();
            }
        }

        #endregion

        #region --- Operator ---

        public static implicit operator List<T>(MonitoredList<T> list)
        {
            return list._list;
        }
        
        public static implicit operator MonitoredList<T>(List<T> list)
        {
            return new MonitoredList<T>(list);
        }

        #endregion

        #region --- Ctor ---

        public MonitoredList()
        {
            _list = new List<T>();
        }
        
        public MonitoredList(int capacity)
        {
            _list = new List<T>(capacity);
        }
        
        public MonitoredList(List<T> list)
        {
            _list = list;
        }
        
        public MonitoredList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
        }
        
        #endregion
    }
}