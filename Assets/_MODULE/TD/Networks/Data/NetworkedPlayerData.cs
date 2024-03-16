using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace TD.Networks.Data
{
    public class NetworkedPlayerData : INetworkSerializable
    {
        public string name;
        public ulong id;
        public int score;

        public NetworkedPlayerData() { } 
        public NetworkedPlayerData(string name, ulong id, int score = 0) 
        { 
            this.name = name; 
            this.id = id; 
            this.score = score; 
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref score);
        }

    }
}

