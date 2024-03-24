using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerSingleMode : MonoBehaviour, ICharacter
{
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float stamina = 100;

    [SerializeField]
    private float staminaCost = 5;

    [SerializeField]
    private float staminaRecovery = 10;

    [SerializeField]
    private float timeToRecover = 2f;

    private enum MovementState
    {
        Standing,
        Walking,
        Running
    }

    private MovementState currentMovementState = MovementState.Standing;
    private bool isRecovering = false;

    public event Action OnAction;

    private void Start()
    {
        playerTransform = transform;
        RecoverStamina();
        OnAction += HandleAction;
    }

    private void Update()
    {
        if (!(GameplayManagerS.Instance.CurrentState == GameplayManagerS.GAME_STATE.PLAYING))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            OnAction();
        }
    }

    private void FixedUpdate()
    {
        if (!(GameplayManagerS.Instance.CurrentState == GameplayManagerS.GAME_STATE.PLAYING))
            return;

        NormalRun();
    }

    public void NormalRun()
    {
        if (isRecovering)
            return;
        playerTransform.position += new Vector3(stamina * 0.01f * Time.fixedDeltaTime, 0, 0);
    }

    private IEnumerator RecoverStaminaCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (stamina < 100)
            {
                stamina += staminaRecovery;
                stamina = Mathf.Clamp(stamina, 0, 100);
            }
        }
    }

    private void RecoverStamina()
    {
        StartCoroutine(RecoverStaminaCoroutine());
    }

    public void HandleAction()
    {
        if (isRecovering)
            return;

        UseStamina();
        HandleMovementState();
    }

    private MovementState GetMovementState()
    {
        if (stamina <= 0)
            return MovementState.Standing;
        else if (stamina < 50)
            return MovementState.Walking;
        else
            return MovementState.Running;
    }

    private void HandleMovementState()
    {
        currentMovementState = GetMovementState();
        switch (currentMovementState)
        {
            case MovementState.Standing:
                break;
            case MovementState.Walking:
                MovePlayer(speed);
                break;
            case MovementState.Running:
                MovePlayer(speed * 2);
                break;
        }
    }

    private void MovePlayer(float currentSpeed)
    {
        Vector3 targetPosition =
            playerTransform.position + new Vector3(currentSpeed * Time.deltaTime, 0, 0);
        playerTransform.DOMove(targetPosition, 0.1f);
    }

    public void UseStamina()
    {
        if (stamina > 0)
        {
            stamina -= staminaCost;
            if (stamina <= 0)
            {
                isRecovering = true;
                stamina = 0;
                Invoke(nameof(OnRecoverDone), timeToRecover);
            }
        }
    }

    private void OnRecoverDone()
    {
        isRecovering = false;
    }

    public void SetPlayerColor(GameSpawner.PLAYER_COLOR color)
    {
        Renderer playerRenderer = GetComponentInChildren<Renderer>();
        playerRenderer.material.color = GetColor(color);
    }

    private Color GetColor(GameSpawner.PLAYER_COLOR color)
    {
        switch (color)
        {
            case GameSpawner.PLAYER_COLOR.RED:
                return Color.red;
            case GameSpawner.PLAYER_COLOR.BLUE:
                return Color.blue;
            case GameSpawner.PLAYER_COLOR.GREEN:
                return Color.green;
            case GameSpawner.PLAYER_COLOR.YELLOW:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    public float CurrentPercentageStamina()
    {
        return stamina / 100f;
    }
}
