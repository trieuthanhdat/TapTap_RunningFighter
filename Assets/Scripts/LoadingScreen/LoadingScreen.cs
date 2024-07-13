using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TD.UServices.Authentication;
using TD.UServices.Core;
using System;

public class LoadingScreen : MonoBehaviour, ILoadingService
{
    public enum StepLoadingService
    {
        NONE = 0,
        STEP_LOAD_UNITY_SERVICE = 1,
        STEP_UNITY_AUTHENTICATE = 2,
        FINALSTEP_LOAD_SCENE_ASYNC = 3,
    }

    [SerializeField] private Slider _loadingSlider;
    [Header("DEBUG SECTION")]
    [SerializeField] private StepLoadingService _stepLoadingService;

    private IEnumerator _currentLoadingRoutine;

    private void Start()
    {
        StartFirstStep();
    }

    private void StartFirstStep()
    {
        _stepLoadingService = StepLoadingService.STEP_LOAD_UNITY_SERVICE;
        LoadGameAsync();
        // LoadLoginScene();
    }

    private void LoadGameAsync()
    {
        switch (_stepLoadingService)
        {
            case StepLoadingService.STEP_LOAD_UNITY_SERVICE:
                _currentLoadingRoutine = Step_LoadUnityService();
                break;
            case StepLoadingService.STEP_UNITY_AUTHENTICATE:
                _currentLoadingRoutine = Step_SignInUnityService();
                break;
            case StepLoadingService.FINALSTEP_LOAD_SCENE_ASYNC:
                _currentLoadingRoutine = Step_LoadSceneAsync(SceneController.Instance.CurrentScene + 1);
                break;
        }
        StartCoroutine(_currentLoadingRoutine);
    }

    private void LoadLoginScene(){
        string sceneName = "LoginScene";
        SceneManager.LoadScene(sceneName);
    }


    private void ProcessNextStep()
    {
        _stepLoadingService += 1;
        LoadGameAsync();
    }

    private void UpdateLoadingProgress(float step, Action stepCallback = null)
    {
        float progress = step / ILoadingService.GetCountILoadingServiceMethods();
        if (_loadingSlider) _loadingSlider.DOValue(progress, 0.5f).OnComplete(()=> stepCallback?.Invoke());
        Debug.Log($"{nameof(LoadingScreen).ToUpper()}: Loading progress: {progress}");
    }

    #region _____INTERFACE IMPLEMENTATION_____
    public IEnumerator Step_LoadSceneAsync(int sceneId)
    {
        yield return EnterGame(sceneId);
    }

    public IEnumerator Step_LoadUnityService()
    {
        // Wait until Unity service is synchronized
        UnityServicesManager.Instance.Initialize();
        while (!UnityServicesManager.Instance.IsUnityServiceSync)
        {
            yield return null;
        }
        UpdateLoadingProgress((int)_stepLoadingService);
        ProcessNextStep();
    }

    public IEnumerator Step_SignInUnityService()
    {
        // Wait until the user is signed in
        if(!UnityAuthenticationManager.instance.IsSignIn &&
            UnityServicesManager.instance.SignInAfterInit)
        {
            UnityAuthenticationManager.Instance.StartSignIn();
            while (!UnityAuthenticationManager.Instance.IsSignIn)
            {
                yield return null;
            }
        }
       
        UpdateLoadingProgress((int)_stepLoadingService);
        ProcessNextStep();
    }
    #endregion
    private IEnumerator EnterGame(int index)
    {
        if (_stepLoadingService != StepLoadingService.FINALSTEP_LOAD_SCENE_ASYNC)
        {
            StartFirstStep();
            yield break;
        }
        int lastStep = ILoadingService.GetCountILoadingServiceMethods();
        UpdateLoadingProgress(lastStep, ()=>
        {
            SceneManager.LoadSceneAsync(index);
        });
    }

    
}
