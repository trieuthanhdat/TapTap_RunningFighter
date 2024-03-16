using TD.Networks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TD.Networks.GamePlay
{
    public class NetworkedScorer : NetworkBehaviour
    {
        [SerializeField] NetworkedDataService m_dataStore = default;
        ulong m_localId;
        [SerializeField] TMP_Text m_scoreOutputText = default;

        [Tooltip("When the game ends, this will be called once for each player in order of rank (1st-place first, and so on).")]
        [SerializeField] UnityEvent<NetworkedPlayerData> m_onGameEnd = default;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        // Called on the host.
        public void ScoreSuccess(ulong id)
        {
            int newScore = m_dataStore.UpdateScore(id, 1);
            UpdateScoreOutput_ClientRpc(id, newScore);
        }
        public void ScoreFailure(ulong id)
        {
            int newScore = m_dataStore.UpdateScore(id, -1);
            UpdateScoreOutput_ClientRpc(id, newScore);
        }

        [ClientRpc]
        void UpdateScoreOutput_ClientRpc(ulong id, int score)
        {
            if (m_localId == id)
                m_scoreOutputText.text = score.ToString("00");
        }
    }

}
