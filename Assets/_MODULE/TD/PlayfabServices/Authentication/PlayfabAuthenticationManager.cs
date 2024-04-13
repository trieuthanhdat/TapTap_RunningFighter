using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabAuthenticationManager : MonoSingleton<PlayfabAuthenticationManager>
{
    private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;
    // private void OnLoginWithUsername(string username, string password) {
    //     _AuthService.Username = username;
    //     _AuthService.Password = password;
    //     _AuthService.Authenticate(Authtypes.UsernameAndPassword);
    // }

    [SerializeField] private bool ClearPlayerPrefs = false;

    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    [Header("DEBUG SECTION")]
    [SerializeField] private string _email;
    [SerializeField] private string _password;
    [SerializeField] private string _player_name;

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

    void Start()
    {
        Debug.Log("PlayfabAuthenticationManager Start");


        // Subscribe to events that happen after authentication
        PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
        PlayFabAuthService.OnPlayFabError += OnPlayFabError;

        _AuthService.InfoRequestParams = InfoRequestParams;
        _AuthService.Authenticate();
    }

    // test login
    [ContextMenu("LoginWithEmail")]
    public void LoginWithEmail()
    {
        OnLoginWithEmail(_email, _password);
    }

    // test register
    [ContextMenu("Register")]
    public void Register()
    {
        OnRegister(_email, _password);
    }

    // test play as guest
    [ContextMenu("PlayAsGuest")]
    public void PlayAsGuest()
    {
        OnPlayAsGuest();
    }
    
    // set player name
    [ContextMenu("SetPlayerDisplayName")]
    public void SetPlayerDisplayName(){
        SetPlayerDisplayName(_player_name);
    }

    // test cloud script
    [ContextMenu("Get Hello World")]
    public void GetHelloWorld(){
        PlayfabManager.ExecuteCloudScript("hello", null, OnGetHelloWorld, OnError);
    }

    private void OnGetHelloWorld(ExecuteCloudScriptResult result){
        Debug.Log(result.FunctionResult);
    }

    private void OnDisplayAuthentication()
    {
        Debug.Log("Display Authentication");
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success");
        Debug.Log(result.PlayFabId);
        Debug.Log(result.SessionTicket);
    }

    private void OnPlayFabError(PlayFabError error)
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

    private void OnLoginWithEmail(string email, string password)
    {
        Debug.Log("Login with email and password");
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }
    private void OnPlayAsGuest()
    {
        Debug.Log("Play as guest");
        _AuthService.Authenticate(Authtypes.Silent);
    }

    private void OnRegister(string email, string password)
    {
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Username = _player_name;
        _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    [ContextMenu("Get Player Display Name")]
    public void GetPlayerDisplayName(){
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), result => {
            Debug.Log(result.PlayerProfile.DisplayName);
        }, error => {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void SetPlayerDisplayName(string displayName){
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest(){
            DisplayName = displayName
        }, result => {
            Debug.Log("Display Name Updated");
        }, error => {
            Debug.Log(error.GenerateErrorReport());
        });
    }

    [ContextMenu("Get Player Level")]
    // Get player level in Player Data (Title Data)
    public void GetPlayerLevel(){
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetPlayerLevel, OnError);
    }

    private void OnGetPlayerLevel(GetUserDataResult result){
        if(result.Data.ContainsKey("Level")){
            Debug.Log(result.Data["Level"].Value);
        }
    }

    private void OnError(PlayFabError error){
        Debug.Log(error.GenerateErrorReport());
    }
}
