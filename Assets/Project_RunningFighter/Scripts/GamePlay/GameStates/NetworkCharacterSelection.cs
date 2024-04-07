using Project_RunningFighter.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Avatar = Project_RunningFighter.Data.Avatar;
public class NetworkCharacterSelection : NetworkBehaviour
{
    public enum SeatState : byte
    {
        Inactive,
        Active,
        LockedIn,
    }

    /// Describes one of the players in the lobby, and their current character-select status.
    /// Putting FixedString inside an INetworkSerializeByMemcpy struct is not recommended because it will lose the
    /// bandwidth optimization provided by INetworkSerializable -- an empty FixedString128Bytes serialized normally
    /// or through INetworkSerializable will use 4 bytes of bandwidth, but inside an INetworkSerializeByMemcpy, that
    /// same empty value would consume 132 bytes of bandwidth. 
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;

        private FixedPlayerName m_PlayerName; // I'm sad there's no 256Bytes fixed list :(

        public int PlayerNumber; 
        public int SeatIdx; 
        public float LastChangeTime;

        public SeatState SeatState;


        public LobbyPlayerState(ulong clientId, string name, int playerNumber, SeatState state, int seatIdx = -1, float lastChangeTime = 0)
        {
            ClientId = clientId;
            PlayerNumber = playerNumber;
            SeatState = state;
            SeatIdx = seatIdx;
            LastChangeTime = lastChangeTime;
            m_PlayerName = new FixedPlayerName();

            PlayerName = name;
        }

        public string PlayerName
        {
            get => m_PlayerName;
            private set => m_PlayerName = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref m_PlayerName);
            serializer.SerializeValue(ref PlayerNumber);
            serializer.SerializeValue(ref SeatState);
            serializer.SerializeValue(ref SeatIdx);
            serializer.SerializeValue(ref LastChangeTime);
        }

        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId &&
                   m_PlayerName.Equals(other.m_PlayerName) &&
                   PlayerNumber == other.PlayerNumber &&
                   SeatIdx == other.SeatIdx &&
                   LastChangeTime.Equals(other.LastChangeTime) &&
                   SeatState == other.SeatState;
        }
    }

    private NetworkList<LobbyPlayerState> m_LobbyPlayers;

    public Avatar[] AvatarConfiguration;

    private void Awake()
    {
        m_LobbyPlayers = new NetworkList<LobbyPlayerState>();
    }

    /// Current state of all players in the lobby.
    public NetworkList<LobbyPlayerState> LobbyPlayers => m_LobbyPlayers;

    /// When this becomes true, the lobby is closed and in process of terminating (switching to gameplay).
    public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);

    /// Server notification when a client requests a different lobby-seat, or locks in their seat choice
    public event Action<ulong, int, bool> OnClientChangedSeat;

    /// RPC to notify the server that a client has chosen a seat.
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void ServerChangeSeatRpc(ulong clientId, int seatIdx, bool lockedIn)
    {
        OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
    }
}