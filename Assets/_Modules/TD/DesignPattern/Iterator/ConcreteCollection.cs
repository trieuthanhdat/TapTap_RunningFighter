using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.DesignPattern.Iterator 
{
    public class ConcreteCollection<T> : IAbstractCollection<T>
    {
        private List<T> m_ListCollection = new List<T>();
        public Iterator<T> CreateIterator()
        {
            return new Iterator<T>(this);
        }
        public int Count
        {
            get { return m_ListCollection.Count; }
        }
        public void AddElement(T element)
        {
            m_ListCollection.Add(element);
        }
        public T GetElement(int IndexPosition)
        {
            return m_ListCollection[IndexPosition];
        }
    }
}
