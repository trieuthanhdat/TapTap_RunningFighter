using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatic : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    
}
