using Project_RunningFighter.Gameplay.GameplayObjects;
using System;
using Unity.Multiplayer.Samples.Utilities;
using UnityEngine;

namespace Project_RunningFighter.Gameplay.UI
{
    public class ClientBossRoomLoadingScreen : ClientLoadingScreen
    {
        [SerializeField]
        PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

        protected override void AddOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker)
        {
            base.AddOtherPlayerProgressBar(clientId, progressTracker);
            m_LoadingProgressBars[clientId].NameText.text = GetPlayerName(clientId);
        }

        protected override void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            base.UpdateOtherPlayerProgressBar(clientId, progressBarIndex);
            m_LoadingProgressBars[clientId].NameText.text = GetPlayerName(clientId);
        }

        string GetPlayerName(ulong clientId)
        {
            foreach (var player in m_PersistentPlayerRuntimeCollection.Items)
            {
                if (clientId == player.OwnerClientId)
                {
                    return player.NetworkNameState.Name.Value;
                }
            }
            return "";
        }
    }
}
