using Project_RunningFighter.Infrastruture;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TD.UServices.CoreLobby.Utilities;
using TD.UServices.Infrastructure;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Utils;
using VContainer;

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
        protected const int k_InitTimeout = 10000;
        protected static bool s_IsSigningIn;

        [SerializeField] private TextAsset unityAuthenticationBackupData;
        [SerializeField] UnityAuthenticationConfig unityAuthenticationConfig;
        [Inject]
        IPublisher<UnityServiceErrorMessage> m_UnityServiceErrorMessagePublisher;

        public static EventHandler OnSignInSuccessfully;
        public static EventHandler OnNameChanged;

        #region ____PROPERTIES____
        private string _playerID = "";
        public string PlayerID
        {
            private set => _playerID = value;
            get => _playerID;
        }
        private string _playerName = "";
        public string PlayerName
        {
            private set => _playerName = value;
            get => _playerName;
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
            private string playerName;
            public string GetPlayerID()
            {
                return playerID;
            }
            public string GetPlayerName()
            {
                return playerName;
            }
            public AuthenticationInforArgs(string pID, string pName)
            {
                playerID = pID;
                playerName = pName;
            }
        }
        private void Start()
        {
            string json = JsonUtility.ToJson(unityAuthenticationConfig);
            Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: json {json}");

            StartCoroutine(Co_GetAuthenticationConfig());
        }
#if UNITY_EDITOR
        private async void Update()
        {
            //if(Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    await SwitchProfile();
                }
            }
        }
#endif
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
        public async Task<bool> EnsurePlayerIsAuthorized()
        {
            if (AuthenticationService.Instance.IsAuthorized)
            {
                return true;
            }

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                //not rethrowing for authentication exceptions - any failure to authenticate is considered "handled failure"
                return false;
            }
            catch (Exception e)
            {
                //all other exceptions should still bubble up as unhandled ones
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                throw;
            }
        }
        private IEnumerator SignInAnonymouslyAsyncCoroutine()
        {
            /*if(UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions options = new InitializationOptions();
                //Can parse player name here
                options.SetProfile(UnityEngine.Random.Range(0, 100000).ToString());
                UnityServices.InitializeAsync(options);
            }*/
            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            while (!signInTask.IsCompleted)
            {
                yield return null;
            }
            _isSigned = true;
            
        }
        private async Task SwitchProfile()
        {
            string serviceProfileName = "testProfile";
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{LocalProfileTool.LocalProfileSuffix}";
#endif
            AuthenticationService.Instance.SwitchProfile(serviceProfileName);

            await UnityAutenticationManager.TrySignInAsync(serviceProfileName);
            Debug.Log($"Switch profile: {AuthenticationService.Instance.Profile.ToString()}");

        }
        public static async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return true;

            //Another Service is mid-initialization:
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out

                return UnityServices.State == ServicesInitializationState.Initialized;
            }

            if (profileName != null)
            {
                //ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions().SetProfile(profileName);

                //If you are using multiple unity services, make sure to initialize it only once before using your services.
                await UnityServices.InitializeAsync(authProfile);
            }
            else
                await UnityServices.InitializeAsync();

            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized)
                    await Task.Delay(100);
            }
        }
        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            if (!await TryInitServicesAsync(profileName))
                return false;
            if (s_IsSigningIn)
            {
                var task = WaitForSignedIn();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out
                return AuthenticationService.Instance.IsSignedIn;
            }

            s_IsSigningIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            s_IsSigningIn = false;

            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                    await Task.Delay(100);
            }
        }
        #region ___PLAYER NAME METHODS___
        public void ChangePlayerNameAsync(string newName)
        {
            AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            OnNameChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
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
            _playerName = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName) ? 
                          NameGenerator.GetName(_playerID) :
                          AuthenticationService.Instance.PlayerName;

            OnSignInSuccessfully?.Invoke(this, new AuthenticationInforArgs(_playerID, _playerName));
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"{nameof(UnityAutenticationManager).ToUpper()}: player id => {_playerID}");
        }
        #endregion
    }

}
