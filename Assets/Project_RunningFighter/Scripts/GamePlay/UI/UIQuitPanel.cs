using Project_RunningFighter.ConnectionManagement;
using Project_RunningFighter.Infrastruture;
using System;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UIQuitPanel : MonoBehaviour
    {
        enum QuitMode
        {
            ReturnToMenu,
            QuitApplication
        }

        [SerializeField] QuitMode m_QuitMode = QuitMode.ReturnToMenu;

        [Inject] ConnectionManager m_ConnectionManager;

        [Inject] IPublisher<QuitApplicationMessage> m_QuitApplicationPub;

        public void Quit()
        {
            switch (m_QuitMode)
            {
                case QuitMode.ReturnToMenu:
                    m_ConnectionManager.RequestShutdown();
                    break;
                case QuitMode.QuitApplication:
                    m_QuitApplicationPub.Publish(new QuitApplicationMessage());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gameObject.SetActive(false);
        }
    }
    public struct QuitApplicationMessage { }
}
