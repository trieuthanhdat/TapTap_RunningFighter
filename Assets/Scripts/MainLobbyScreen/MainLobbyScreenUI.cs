using UnityEngine;
using UnityEngine.UI;

public class MainLobbyScreenUI : MonoBehaviour
{
    [SerializeField]
    private Button singleModeButton;

    [SerializeField]
    private Button createRoomButton;

    [SerializeField]
    private Button joinRoomButton;

    void Awake()
    {
        singleModeButton.onClick.AddListener(OnSingleModeButtonClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
    }

    private void OnSingleModeButtonClicked()
    {
        Debug.Log("Single Mode Button Clicked");
        // NetworkManager.Singleton.StartHost();
        SceneController.Instance.NextScene();
    }

    private void OnCreateRoomButtonClicked()
    {
        Debug.Log("Create Room Button Clicked");
        // NetworkManager.Singleton.StartHost();
    }

    private void OnJoinRoomButtonClicked()
    {
        Debug.Log("Join Room Button Clicked");
        // NetworkManager.Singleton.StartClient();
    }

}
