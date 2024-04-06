using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Unity.VisualScripting;

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

    public void Awake() {
        Debug.Log("PlayfabAuthenticationManager Awake");
        if (ClearPlayerPrefs){
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
        }
    }

    void Start() {
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
    public void LoginWithEmail() {
        OnLoginWithEmail(_email, _password);
    }

    // test register
    [ContextMenu("Register")]
    public void Register() {
        OnRegister(_email, _password);
    }

    // test play as guest
    [ContextMenu("PlayAsGuest")]
    public void PlayAsGuest() {
        OnPlayAsGuest();
    }

    private void OnDisplayAuthentication() {
        Debug.Log("Display Authentication");
    }

    private void OnLoginSuccess(LoginResult result) {
        Debug.Log("Login Success");
        Debug.Log(result.PlayFabId);
        Debug.Log(result.SessionTicket);
    }

    private void OnPlayFabError(PlayFabError error) {
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

    private void OnLoginWithEmail(string email, string password) {
        Debug.Log("Login with email and password");
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }
    private void OnPlayAsGuest() {
        Debug.Log("Play as guest");
        _AuthService.Authenticate(Authtypes.Silent);
    }

    private void OnRegister(string email, string password) {
        _AuthService.Email = email;
        _AuthService.Password = password;
        _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }
}
