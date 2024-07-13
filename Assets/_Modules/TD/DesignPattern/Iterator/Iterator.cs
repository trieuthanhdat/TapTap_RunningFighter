using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.DesignPattern.Iterator
{
    public class Iterator<T> : IAbstractIterator<T>
    {
        private ConcreteCollection<T> Collection;
        private int Current = -1; // Start from -1 to support MoveToFirst() operation.
        private readonly int Step = 1;

        // Constructor
        public Iterator(ConcreteCollection<T> collection)
        {
            this.Collection = collection;
        }

        public T First()
        {
            // Setting Current as -1 to access the First Element of the Sequence
            Current = -1;
            return Next();
        }

        // Gets Next Item from the Collection
        public T Next()
        {
            Current += Step;
            if (!IsCompleted)
            {
                return Collection.GetElement(Current);
            }
            else
            {
                return default; // Return default value when iteration is completed
            }
        }

        // Check if the iteration is completed
        public bool IsCompleted
        {
            get { return Current >= Collection.Count - 1; } // Compare with Count - 1
        }

        // Reset the iterator to its initial state
        public void Reset()
        {
            Current = -1;
        }

        // Get the total count of elements in the iteration
        public int Count()
        {
            return Collection.Count;
        }

        // Move the iterator to a specific index
        public bool MoveTo(int index)
        {
            if (index >= 0 && index < Collection.Count)
            {
                Current = index - 1; // Move to index - 1 to support Next() operation
                return true;
            }
            return false;
        }

        // Move the iterator forward by a specified number of steps
        public bool MoveToNext(int step)
        {
            int newIndex = Current + step;
            if (newIndex >= 0 && newIndex < Collection.Count)
            {
                Current = newIndex - 1; // Move to newIndex - 1 to support Next() operation
                return true;
            }
            return false;
        }

        // Move the iterator backward by a specified number of steps
        public bool MoveToPrevious(int step)
        {
            int newIndex = Current - step;
            if (newIndex >= 0 && newIndex < Collection.Count)
            {
                Current = newIndex - 1; // Move to newIndex - 1 to support Next() operation
                return true;
            }
            return false;
        }

        // Get the current item without advancing the iterator
        public T CurrentItem
        {
            get
            {
                if (Current >= 0 && Current < Collection.Count)
                {
                    return Collection.GetElement(Current);
                }
                return default; // Return default value if Current is out of range
            }
        }
    }

}
