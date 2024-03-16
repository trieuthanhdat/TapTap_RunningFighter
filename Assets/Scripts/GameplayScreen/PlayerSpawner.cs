using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    // public Transform[] spawnPoints;

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
    }

    private void SceneLoaded(string sceneName, LoadSceneMode sceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimeout)
    {
        if (IsHost && sceneName == "GameplayScene")
        {
            foreach (ulong id in clientsCompleted)
            {
                SpawnPlayer(id);
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log("Spawning player for client: " + clientId);
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
