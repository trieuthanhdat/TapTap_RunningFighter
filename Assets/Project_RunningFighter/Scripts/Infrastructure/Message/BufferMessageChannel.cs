
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Infrastruture
{
    public class BufferMessageChannel<T> : MessageChannel<T>, IBufferedMessageChannel<T>
    {
        public override void Publish(T message)
        {
            HasBufferedMessage = true;
            BufferedMessage = message;
            base.Publish(message);
        }

        public override IDisposable Subscribe(Action<T> handler)
        {
            var subscription = base.Subscribe(handler);

            if (HasBufferedMessage)
            {
                handler?.Invoke(BufferedMessage);
            }

            return subscription;
        }

        public bool HasBufferedMessage { get; private set; } = false;
        public T BufferedMessage { get; private set; }
    }
}
