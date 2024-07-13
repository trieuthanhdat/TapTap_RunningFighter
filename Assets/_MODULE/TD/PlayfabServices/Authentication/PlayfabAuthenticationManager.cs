using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabAuthenticationManager : MonoSingleton<PlayfabAuthenticationManager>
{
    private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;
    public PlayFabAuthService AuthService => _AuthService;

    [SerializeField] private bool ClearPlayerPrefs = false;

    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    public void Awake()
    {
        Debug.Log("PlayfabAuthenticationManager Awake");
        if (ClearPlayerPrefs)
        {
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
        }
    }

    public void OnGetHelloWorld(ExecuteCloudScriptResult result)
    {
        Debug.Log(result.FunctionResult);
    }

    public void OnDisplayAuthentication()
    {
        Debug.Log("Display Authentication");
    }

    public void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success");
        Debug.Log(result.PlayFabId);
        Debug.Log(result.SessionTicket);
    }

    public void OnPlayFabError(PlayFabError error)
    {
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
            case PlayFabErrorCode.InvalidPassword:
            case PlayFabErrorCode.InvalidEmailOrPassword:
                Debug.Log("Invalid email or password");
                break;

            case PlayFabErrorCode.AccountNotFound:
                Debug.Log("Account not found");
                break;
            default:
                break;
        }

        Debug.Log(error.Error);
        Debug.Log(error.GenerateErrorReport());
    }

    public void OnLoginWithEmail(string email, string password)
    {
        Debug.Log("Login with email and password");
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }

    public void OnLoginWithFacebook()
    {
        _AuthService.Authenticate(Authtypes.Facebook);
    }

    public void OnPlayAsGuest()
    {
        Debug.Log("Play as guest");
        _AuthService.Authenticate(Authtypes.Silent);
    }
    public void OnRegister(string email, string password)
    {
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }
    public void SetPlayerDisplayName(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = displayName
        }, result =>
        {
            Debug.Log("Display Name Updated");
        }, error =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void OnError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    [ContextMenu("Get Player Display Name")]
    // public void GetPlayerName()
    public string GetPlayerName()
    {
        string displayName = "";
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), result =>
        {
            Debug.Log(result.PlayerProfile.DisplayName);
            displayName = result.PlayerProfile.DisplayName;
        }, error =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
        return displayName;
    }

    [ContextMenu("Get Player Combined Info")]
    public string GetPlayerID()
    {
        string playerID = "";
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), result =>
        {
            Debug.Log(result.PlayerProfile.PlayerId);
            playerID = result.PlayerProfile.PlayerId; // master ID
        }, error =>
        {
            Debug.Log(error.GenerateErrorReport());
        });
        return playerID;
    }

}
