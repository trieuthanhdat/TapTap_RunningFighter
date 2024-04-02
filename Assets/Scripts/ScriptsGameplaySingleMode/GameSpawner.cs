using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject botPrefab;
    // [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerSpawnPoints;

    public enum PLAYER_COLOR { RED, BLUE, GREEN, YELLOW }

    void Start()
    {
        SpawnPlayer();
        SpawnStaminaBar();
    }

    void SpawnPlayer()
    {
        for (int i = 0; i < playerSpawnPoints.transform.childCount; i++)
        {
            GameObject player = Instantiate(i == 0 ? playerPrefab : botPrefab);
            player.transform.position = playerSpawnPoints.transform.GetChild(i).position;

            switch (i)
            {
                case 0:
                    player.GetComponent<ICharacter>().SetPlayerColor(PLAYER_COLOR.RED);
                    break;
                case 1:
                    player.GetComponent<ICharacter>().SetPlayerColor(PLAYER_COLOR.BLUE);
                    break;
                case 2:
                    player.GetComponent<ICharacter>().SetPlayerColor(PLAYER_COLOR.GREEN);
                    break;
                case 3:
                    player.GetComponent<ICharacter>().SetPlayerColor(PLAYER_COLOR.YELLOW);
                    break;
            }
        }
    }

    void SpawnStaminaBar()
    {
        // Get all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log("Player: " + players[i].name);
            StaminaManager.Instance.CreateStaminaBar(players[i].GetComponent<ICharacter>());
        }
    }

}

