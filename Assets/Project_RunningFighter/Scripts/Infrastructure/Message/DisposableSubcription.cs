using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Infrastruture
{
    public class DisposableSubcription<T> : IDisposable
    {
        Action<T> m_Handler;
        bool m_IsDisposed;
        IMessageChannel<T> m_MessageChannel;

        public DisposableSubcription(IMessageChannel<T> messageChannel, Action<T> handler)
        {
            m_MessageChannel = messageChannel;
            m_Handler = handler;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (!m_MessageChannel.IsDisposed)
                {
                    m_MessageChannel.Unsubscribe(m_Handler);
                }

                m_Handler = null;
                m_MessageChannel = null;
            }
        }
    }
}
