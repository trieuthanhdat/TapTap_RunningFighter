﻿//--------------------------------------------------------------------------------------
// LoginWindowView.cs
//
// Advanced Technology Group (ATG)
// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//--------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginWindowView : BaseManager<LoginWindowView>
{
    // Debug Flag to simulate a reset
    public bool ClearPlayerPrefs;

    public GameObject LoginStackPanel;

    // Meta fields for objects in the UI
    public InputField Username;
    public InputField Password;
    public InputField ConfirmPassword;
    public Toggle RememberMe;

    public Button LoginButton;
    public Button PlayAsGuestButton;
    public Button RegisterButton;
    public Button CancelRegisterButton;

    // Meta references to panels we need to show / hide
    public GameObject RegisterPanel;
    public GameObject SigninPanel;
    public GameObject LoginPanel;
    // public GameObject LoggedinPanel;
    public GameObject CreateDisplayNamePanel;
    public InputField DisplayNameInput;
    public Button DisplayNameButton;
    public Text StatusText;

    public Button LoginWithFacebookButton;

    // Settings for what data to get from playfab on login.
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    // Reference to our Authentication service
    private PlayFabAuthService _AuthService;

    public void Awake()
    {
        _AuthService = PlayfabAuthenticationManager.Instance.AuthService;

        if (ClearPlayerPrefs)
        {
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
        }

        //Set our remember me button to our remembered state.
        RememberMe.isOn = _AuthService.RememberMe;

        //Subscribe to our Remember Me toggle
        RememberMe.onValueChanged.AddListener(
            (toggle) =>
            {
                _AuthService.RememberMe = toggle;
            });
    }

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Start()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        SigninPanel.SetActive(true);

        // Subscribe to events that happen after we authenticate
        PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
        PlayFabAuthService.OnPlayFabError += OnPlayFaberror;

        // Bind to UI buttons to perform actions when user interacts with the UI.
        LoginButton.onClick.AddListener(OnLoginClicked);
        PlayAsGuestButton.onClick.AddListener(OnPlayAsGuestClicked);
        RegisterButton.onClick.AddListener(OnRegisterButtonClicked);
        CancelRegisterButton.onClick.AddListener(OnCancelRegisterButtonClicked);
        LoginWithFacebookButton.onClick.AddListener(OnLoginWithFacebookClicked);

        _AuthService.InfoRequestParams = InfoRequestParams;
        _AuthService.Authenticate();
    }


    /// <summary>
    /// Login Successfully - Goes to next screen.
    /// </summary>
    /// <param name="result"></param>
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        Debug.LogFormat("Logged In as: {0}", result.PlayFabId);
        StatusText.text = "";
        LoginPanel.SetActive(false);
        OnSetDisplayName();
    }

    /// <summary>
    /// Error handling for when Login returns errors.
    /// </summary>
    /// <param name="error"></param>
    private void OnPlayFaberror(PlayFabError error)
    {
        //There are more cases which can be caught, below are some
        //of the basic ones.
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
            case PlayFabErrorCode.InvalidPassword:
            case PlayFabErrorCode.InvalidEmailOrPassword:
                StatusText.text = "Invalid Email or Password";
                break;

            case PlayFabErrorCode.AccountNotFound:
                RegisterPanel.SetActive(true);
                SigninPanel.SetActive(false);
                return;
            default:
                StatusText.text = error.GenerateErrorReport();
                break;
        }

        //Also report to debug console, this is optional.
        Debug.Log(error.Error);
        Debug.LogError(error.GenerateErrorReport());
    }

    /// <summary>
    /// Choose to display the Auth UI or any other action.
    /// </summary>
    private void OnDisplayAuthentication()
    {
        //Here we have choses what to do when AuthType is None.
        LoginPanel.SetActive(true);
        // LoggedinPanel.SetActive(false);
        StatusText.text = "";

        /*
         * Optionally we could Not do the above and force login silently
         * 
         * _AuthService.Authenticate(Authtypes.Silent);
         * 
         * This example, would auto log them in by device ID and they would
         * never see any UI for Authentication.
         * 
         */
    }

    /// <summary>
    /// Play As a guest, which means they are going to silently authenticate
    /// by device ID or Custom ID
    /// </summary>
    private void OnPlayAsGuestClicked()
    {
        StatusText.text = "Logging In As Guest ...";
        PlayfabAuthenticationManager.Instance.OnPlayAsGuest();
        // _AuthService.Authenticate(Authtypes.Silent);
    }

    /// <summary>
    /// Login Button means they've selected to submit a username (email) / password combo
    /// Note: in this flow if no account is found, it will ask them to register.
    /// </summary>
    private void OnLoginClicked()
    {
        StatusText.text = string.Format("Logging In As {0} ...", Username.text);
        PlayfabAuthenticationManager.Instance.OnLoginWithEmail(Username.text, Password.text);
        // _AuthService.Email = Username.text;
        // _AuthService.Password = Password.text;
        // _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }

    /// <summary>
    /// No account was found, and they have selected to register a username (email) / password combo.
    /// </summary>
    private void OnRegisterButtonClicked()
    {
        if (Password.text != ConfirmPassword.text)
        {
            StatusText.text = "Passwords do not Match.";
            return;
        }

        StatusText.text = string.Format("Registering User {0} ...", Username.text);
        PlayfabAuthenticationManager.Instance.OnRegister(Username.text, Password.text);

        // _AuthService.Email = Username.text;
        // _AuthService.Password = Password.text;
        // _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    /// <summary>
    /// They have opted to cancel the Registration process.
    /// Possibly they typed the email address incorrectly.
    /// </summary>
    private void OnCancelRegisterButtonClicked()
    {
        // Reset all forms
        Username.text = string.Empty;
        Password.text = string.Empty;
        ConfirmPassword.text = string.Empty;
        // Show panels
        RegisterPanel.SetActive(false);
        SigninPanel.SetActive(true);
    }

    /// <summary>
    /// They have opted to login with Facebook.
    /// </summary>
    private void OnLoginWithFacebookClicked()
    {
        Debug.Log("Logging in with Facebook");
        _AuthService.Authenticate(Authtypes.Facebook);
    }

    // after login, we need to set the display name
    // check if the player has a display name, if not, ask them to set one.
    public void OnSetDisplayName()
    {
        // get display name from PlayerProfile
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), (result) =>
        {
            Debug.Log(result);
            if (result.PlayerProfile.DisplayName == null)
            {
                LoginStackPanel.SetActive(false);
                CreateDisplayNamePanel.SetActive(true);
                DisplayNameButton.onClick.AddListener(OnDisplayNameSubmit);
            }
            else
            {
                LoginStackPanel.SetActive(true);
                CreateDisplayNamePanel.SetActive(false);
                LoadingMenuScene();
            }
        }, (error) =>
        {
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void OnDisplayNameSubmit()
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = DisplayNameInput.text };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
        {
            Debug.Log("Updated Display Name");
        }, (error) =>
        {
            Debug.LogError(error.GenerateErrorReport());
        });
        LoadingMenuScene();
    }
    private void LoadingMenuScene()
    {
        string sceneName = "MainMenuUI";
        SceneManager.LoadSceneAsync(sceneName);
    }
}