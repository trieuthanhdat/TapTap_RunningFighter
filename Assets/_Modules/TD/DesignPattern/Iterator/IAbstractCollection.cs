using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.DesignPattern.Iterator
{
    public interface IAbstractCollection<T> 
    {
        public Iterator<T> CreateIterator();
    }
}
