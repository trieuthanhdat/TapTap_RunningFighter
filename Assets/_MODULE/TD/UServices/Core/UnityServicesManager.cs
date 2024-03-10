using System;
using System.Collections;
using TD.UServices.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace TD.UServices.Core
{
    public class UnityServicesManager : MonoSingleton<UnityServicesManager>
    {
        [SerializeField]
        private UnityAutenticationManager unityAutenticationManager = null;
        [SerializeField] 
        private bool signInAfterInit = false;

        private bool _isServiceInited = false;
        public bool IsUnityServiceSync
        {
            get => _isServiceInited;
        }
        private void Awake()
        {
            if(unityAutenticationManager == null)
            {
                unityAutenticationManager = UnityAutenticationManager.instance;
            }
        }
        public void Initialize()
        {
            StartCoroutine(InitializeCoroutine());
        }

        private IEnumerator InitializeCoroutine()
        {
            yield return StartCoroutine(Co_UnityServicesInitialize());
            if(signInAfterInit) 
                yield return StartCoroutine(unityAutenticationManager.Co_SignIn());
        }

        private IEnumerator Co_UnityServicesInitialize()
        {
            var unityServicesInitialization = UnityServices.InitializeAsync();
            while (!unityServicesInitialization.IsCompleted)
            {
                yield return null;
            }
            // Check for errors if needed
            _isServiceInited = true;
            Debug.Log($"{nameof(UnityServicesManager).ToUpper()}: Unity Services initialized!");
        }
    }

}
