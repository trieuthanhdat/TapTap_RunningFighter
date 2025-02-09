using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Infrastruture
{
    public interface IPublisher<T>
    {
        void Publish(T message);
    }

    public interface ISubscriber<T>
    {
        IDisposable Subscribe(Action<T> handler);
        void Unsubscribe(Action<T> handler);
    }

    public interface IMessageChannel<T> : IPublisher<T>, ISubscriber<T>, IDisposable
    {
        bool IsDisposed { get; }
    }

    public interface IBufferedMessageChannel<T> : IMessageChannel<T>
    {
        bool HasBufferedMessage { get; }
        T BufferedMessage { get; }
    }
}
