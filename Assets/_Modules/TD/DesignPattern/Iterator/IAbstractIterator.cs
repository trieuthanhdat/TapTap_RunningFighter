namespace TD.DesignPattern.Iterator
{
    public interface IAbstractIterator<T>
    {
        T First();
        T Next();
        void Reset();                // Reset the iterator to its initial state.
        int Count();                 // Get the total count of elements in the iteration.
        bool MoveTo(int index);      // Move the iterator to a specific index.
        bool MoveToNext(int step);   // Move the iterator forward by a specified number of steps.
        bool MoveToPrevious(int step); // Move the iterator backward by a specified number of steps.
        T CurrentItem { get; }
        bool IsCompleted { get; }
    }
}
