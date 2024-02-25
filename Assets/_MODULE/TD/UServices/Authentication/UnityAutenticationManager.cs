using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace TD.UServices.Authentication
{
    public enum AuthenticationType
    {
        None,
        AnonymouslySignin,
        GooglePlayGames,
        Facebook,
        Steam
    }
    public class UnityAutenticationManager : MonoSingleton<UnityAutenticationManager>, IUnityAuthentication
    {

        [SerializeField] private TextAsset unityAuthenticationBackupData;
        [SerializeField] UnityAuthenticationConfig unityAuthenticationConfig;

        public static EventHandler OnSignInSuccessfully;
        public static EventHandler OnNameChanged;

        #region ____PROPERTIES____
        private string _playerID = "";
        public string PlayerID
        {
            private set => _playerID = value;
            get => _playerID;
        }

        private bool _isSigned = false;
        public bool IsSignIn
        {
            private set => _isSigned = value;
            get => _isSigned;
        }

        private bool _isInited = false;
        public bool IsInited
        {
            private set => _isInited = value;
            get => _isInited;
        }
        #endregion
        public class AuthenticationInforArgs : EventArgs
        {
            private string playerID;
            public string GetPlayerID()
            {
                return playerID;
            }
            public AuthenticationInforArgs(string pID)
            {
                playerID = pID;
            }
        }
        private void Start()
        {
            string json = JsonUtility.ToJson(unityAuthenticationConfig);
            Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: json {json}");

            StartCoroutine(Co_GetAuthenticationConfig());
        }
        private IEnumerator Co_GetAuthenticationConfig()
        {
            yield return new WaitForSeconds(0.1f);
            if (unityAuthenticationConfig     == null &&
                unityAuthenticationBackupData != null)
            {
                Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: fail to get remote data => use backup");
                unityAuthenticationConfig = JsonUtility.FromJson<UnityAuthenticationConfig>(unityAuthenticationBackupData.text);
            }
            _isInited = true;
        }
        public void StartSignIn()
        {
            StartCoroutine(Co_SignIn());
        }
        public IEnumerator Co_SignIn()
        {
            while (_isInited == false)
            {
                yield return new WaitForEndOfFrame();
            }
            AuthenticationService.Instance.SignedIn += HandleSuccessfulSignIn;
            AuthenticationService.Instance.SignInFailed += HandleRequestFailedError;

            IEnumerator signInCoroutine = null;
            if(EnumUtils.TryParse(unityAuthenticationConfig.AuthenticationType, out AuthenticationType authType))
            {
                switch(authType)
                {
                    case AuthenticationType.AnonymouslySignin:
                        signInCoroutine = SignInAnonymouslyAsyncCoroutine();
                        break;
                    default:
                        Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: Not yet implemented this type of authentication {authType}");
                        yield break;
                }
            }
            if(signInCoroutine == null)
            {
                Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: Cannot get Authentication method");
                yield break;
            }

            while (signInCoroutine.MoveNext())
            {
                yield return signInCoroutine.Current;
            }
        }
        private IEnumerator SignInAnonymouslyAsyncCoroutine()
        {
            /*if(UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions options = new InitializationOptions();
                //Can parse player name here
                options.SetProfile(UnityEngine.Random.Range(0, 100000).ToString());
                await UnityServices.InitializeAsync(options);
            }*/
            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            while (!signInTask.IsCompleted)
            {
                yield return null;
            }
            _isSigned = true;
            
        }
        public void ChangePlayerNameAsync(string newName)
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            OnNameChanged?.Invoke(this, EventArgs.Empty);
        }
        #region ___INTERFACE IMPLEMENTATION___
        public void HandleAuthenticationError(AuthenticationException ex)
        {
            Debug.LogError(ex);
        }

        public void HandleRequestFailedError(RequestFailedException ex)
        {
            Debug.LogError(ex);
        }

        public void HandleSuccessfulSignIn()
        {
            _playerID = AuthenticationService.Instance.PlayerId;
            OnSignInSuccessfully?.Invoke(this, new AuthenticationInforArgs(_playerID));
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: player id => {_playerID}");
        }
        #endregion
    }

}
