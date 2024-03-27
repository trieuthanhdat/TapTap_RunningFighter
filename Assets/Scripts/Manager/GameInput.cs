using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class GameInput : BaseManager<GameInput>
{
    public event EventHandler OnInteractAction;
    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    private GameInputController _gameInputController;

    void Awake()
    {
        _gameInputController = new GameInputController();

        _gameInputController.Touch.Enable();

        _gameInputController.Touch.TouchInput.performed += ctx => OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    
}

