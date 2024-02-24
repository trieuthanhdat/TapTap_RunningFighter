using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameplayController : BaseManager<GameplayController>
{
    private StateController _stateController;
    public event Action OnStartLv;

    public override void Init()
    {
        _stateController = new StateController(new Dictionary<Type, IState>
        {
            // { typeof(StartLevelState), new StartLevelState() },
            // { typeof(Win1GameState), new Win1GameState() },
            // { typeof(LoseState), new LoseState() },
            // { typeof(PauseState), new PauseState() },
            // { typeof(NormalState), new NormalState() },
            // { typeof(AdsState), new AdsState() },
        });

        SetEvent();
    }

    private void OnLoadScene(int sceneId)
    {
        if (sceneId > 1)
        {
            SoundManager.Instance.PlaySoundIfNotPlay(Sounds.LevelBGM, true, true, true);

            int lv = SceneManager.GetActiveScene().buildIndex - 1;

            if (lv > DataManager.Instance.UnlockLv)
            {
                DataManager.Instance.UnlockLv = lv;
            }
            DataManager.Instance.SaveGame();

            OnStartLv?.Invoke();
            
        }
        else
        {
        }
    }

    private void OnDestroy()
    {
        SceneController.Instance.OnChangeScene -= OnLoadScene;

    }

    private void SetEvent()
    {
        SceneController.Instance.OnChangeScene += OnLoadScene;
    }

    public T GetState<T>() where T : class, IState => _stateController.GetState<T>();

    public IState GetCurrentState() => _stateController.CurrentState;

    public Type GetCurrentType() => _stateController.CurrentTypeState;
}
