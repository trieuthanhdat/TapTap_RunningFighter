using Project_RunningFighter.Infrastruture;
using System;
using TD.UServices.Infrastructure;
using Unity.Services.Lobbies;
using UnityEngine;
using VContainer;

namespace Project_RunningFighter.Gameplay.UI
{
    public class UnityServicesUIHandler : MonoBehaviour
    {
        ISubscriber<UnityServiceErrorMessage> m_ServiceErrorSubscription;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        [Inject]
        void Initialize(ISubscriber<UnityServiceErrorMessage> serviceError)
        {
            m_ServiceErrorSubscription = serviceError;
            m_ServiceErrorSubscription.Subscribe(ServiceErrorHandler);
        }

        void ServiceErrorHandler(UnityServiceErrorMessage error)
        {
            var errorMessage = error.Message;
            switch (error.AffectedService)
            {
                case UnityServiceErrorMessage.Service.Lobby:
                {
                    HandleLobbyError(error);
                    break;
                }
                case UnityServiceErrorMessage.Service.Authentication:
                {
                    PopupManager.ShowPopupPanel(
                        "Authentication Error",
                        $"{error.OriginalException.Message} \n tip: You can still use the Direct IP connection option.");
                    break;
                }
                default:
                {
                    PopupManager.ShowPopupPanel("Service error: " + error.Title, errorMessage);
                    break;
                }
            }
        }

        void HandleLobbyError(UnityServiceErrorMessage error)
        {
            var exception = error.OriginalException as LobbyServiceException;
            if (exception != null)
            {
                switch (exception.Reason)
                {
                    // If the error is one of the following, the player needs to know about it, so show in a popup message. Otherwise, the log in the console is sufficient.
                    case LobbyExceptionReason.ValidationError:
                        PopupManager.ShowPopupPanel("Validation Error", "Validation check failed on Lobby. Is the join code correctly formatted?");
                        break;
                    case LobbyExceptionReason.LobbyNotFound:
                        PopupManager.ShowPopupPanel("Lobby Not Found", "Requested lobby not found. The join code is incorrect or the lobby has ended.");
                        break;
                    case LobbyExceptionReason.LobbyConflict:
                        // LobbyConflict can have multiple causes. Let's add other solutions here if there's other situations that arise for this.
                        PopupManager.ShowPopupPanel("Failed to join Lobby", "Failed to join Lobby due to a conflict. If trying to connect two local builds to the same lobby, they need to have different profiles. See logs for more details.");
                        break;
                    case LobbyExceptionReason.NoOpenLobbies:
                        PopupManager.ShowPopupPanel("Failed to join Lobby", "No accessible lobbies are currently available for quick-join.");
                        break;
                    case LobbyExceptionReason.LobbyFull:
                        PopupManager.ShowPopupPanel("Failed to join Lobby", "Lobby is full and can't accept more players.");
                        break;
                    case LobbyExceptionReason.Unauthorized:
                        PopupManager.ShowPopupPanel("Lobby error", "Received HTTP error 401 Unauthorized from Lobby Service.");
                        break;
                    case LobbyExceptionReason.RequestTimeOut:
                        PopupManager.ShowPopupPanel("Lobby error", "Received HTTP error 408 Request timed out from Lobby Service.");
                        break;
                }
            }
        }

        void OnDestroy()
        {
            if (m_ServiceErrorSubscription != null)
            {
                m_ServiceErrorSubscription.Unsubscribe(ServiceErrorHandler);
            }
        }
    }
}
