using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TD.UServices.Core
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private Slider _loadingSlider;

        void Start()
        {
            StartCoroutine(LoadSceneAsync(SceneController.Instance.CurrentScene + 1));
            SceneController.Instance.NextScene();
        }

        IEnumerator LoadSceneAsync(int sceneId)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
            UnityServicesManager.instance.Initialize();
            while (!operation.isDone && !UnityServicesManager.instance.IsUnityServiceSync)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                LogSystem.LogByColor("Loading progress: " + progress, "green");
                _loadingSlider.value = progress;
                yield return null;
            }
        }
    }
}
