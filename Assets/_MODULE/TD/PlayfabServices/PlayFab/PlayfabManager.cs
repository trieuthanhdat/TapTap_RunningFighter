using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public static class PlayfabManager
{
    public static bool IsLoggedIn = false;
    public static string UserDisplayName = null;
    // Load the user's display name
    public static void LoadAccountData()
    {
        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest() { },
            (GetAccountInfoResult response) =>
            {
                Debug.Log("Got user display name: " + response.AccountInfo.TitleInfo.DisplayName);
                UserDisplayName = response.AccountInfo.TitleInfo.DisplayName;
                IsAccountInfoLoaded = true;
            },
            (PlayFabError error) =>
            {
                Debug.LogError("GetAccountInfo failed.");
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
    public static bool IsAccountInfoLoaded = false;

    // Update the user's DisplayName
    public static void UpdateDisplayName(string displayName)
    {
        UserDisplayName = displayName;
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            },
            // On success
            (UpdateUserTitleDisplayNameResult response) =>
            {
                Debug.Log("Successfully updated display name to: " + displayName);
            },
            // On failure
            (PlayFabError error) =>
            {
                Debug.LogError("UpdateUserTitleDisplayName failed.");
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    
}
