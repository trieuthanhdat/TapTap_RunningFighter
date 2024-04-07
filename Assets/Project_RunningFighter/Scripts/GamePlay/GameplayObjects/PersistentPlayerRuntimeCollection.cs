using System.Collections;
using System.Collections.Generic;
using Project_RunningFighter.Infrastruture;
using UnityEngine;


namespace Project_RunningFighter.Gameplay.GameplayObjects
{
    //A runtime list of <see cref="PersistentPlayer"/> objects that is populated both on clients and server.
    [CreateAssetMenu]
    public class PersistentPlayerRuntimeCollection : RuntimeCollection<PersistentPlayer>
    {
        public bool TryGetPlayer(ulong clientID, out PersistentPlayer persistentPlayer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (clientID == Items[i].OwnerClientId)
                {
                    persistentPlayer = Items[i];
                    return true;
                }
            }

            persistentPlayer = null;
            return false;
        }
    }
}
