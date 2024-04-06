using Project_RunningFighter.ConnectionManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.ConnectionManagement
{
    abstract class OnlineState : ConnectionState
    {
        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}
