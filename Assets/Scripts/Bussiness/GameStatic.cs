using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatic : MonoBehaviour
{
    void Start()
    {
        SceneController.Instance.NextScene();
        
        DontDestroyOnLoad(gameObject);
    }

    
}
