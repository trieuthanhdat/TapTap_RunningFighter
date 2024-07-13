using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabGetPlayerDataManager : MonoSingleton<PlayfabGetPlayerDataManager>
{
    // get player data in Player Data (Title Data)
    [ContextMenu("GetPlayerData")]
    public void GetPlayerData()
    {
        PlayFabClientAPI.GetPlayerCombinedInfo(new GetPlayerCombinedInfoRequest
        {
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true,
            }
        }, result =>
        {
            Debug.Log("GetPlayerData Success");
            Debug.Log(result.ToJson());
        }, error =>
        {
            Debug.Log("GetPlayerData Error");
            Debug.Log(error.GenerateErrorReport());
        });
    }


    [ContextMenu("GetPlayerData2")]
    public void GetPlayerData2()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest
        {
        }, result =>
        {
            Debug.Log("GetPlayerData2 Success");
            Debug.Log(result.ToJson());
        }, error =>
        {
            Debug.Log("GetPlayerData2 Error");
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
