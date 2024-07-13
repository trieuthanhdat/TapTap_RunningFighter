using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoSingleton<DontDestroyOnLoad>
{
    protected override void OnDestroy()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
