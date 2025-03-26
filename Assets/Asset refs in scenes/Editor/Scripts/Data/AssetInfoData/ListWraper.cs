#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SearchEngine.EditorViews.Data
{
    public class ListWraper<T> : IList<T>
    {
        [SerializeField] protected List<T> data = new List<T>();
       
        public virtual void Add(T item)
        {
            data.Add(item);
        }
        public virtual void Insert(int index, T item)
        {
            data.Insert(index, item);
        }


        public virtual void Clear()
        {
            data.Clear();
        }
        public virtual bool Remove(T item)
        {
            return data.Remove(item);
        }
        public virtual void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }


        public virtual IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual int Count
        {
            get { return data.Count; }
        }
        

        public virtual bool Contains(T item)
        {
            return data.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public virtual bool IsReadOnly
        {
            get { return ((IList)data).IsReadOnly; }
        }
        
        public virtual int IndexOf(T item)
        {
            return data.IndexOf(item);
        }

        public virtual T this[int index]
        {
            get { return data[index];}
            set { data[index] = value; }
        }
    }
}

#endif