using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class GameplayUI : BaseManager<GameplayUI>
{
    [SerializeField] private Button dashButton;

    // event for dash
    public event Action OnPlayerDash;

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        dashButton.onClick.AddListener(() => OnPlayerDash?.Invoke());
    }

}
