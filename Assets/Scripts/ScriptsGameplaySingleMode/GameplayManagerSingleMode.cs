using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameplayManagerSingleMode : MonoBehaviour {
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
    }
}

