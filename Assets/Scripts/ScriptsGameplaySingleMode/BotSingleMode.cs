using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class BotSingleMode : MonoBehaviour, ICharacter
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float stamina = 100;
    [SerializeField] private float staminaCost = 5;
    [SerializeField] private float staminaRecovery = 10;
    [SerializeField] private float timeToRecover = 2f;

    private enum MovementState { Standing, Walking, Running }
    [SerializeField] private MovementState currentMovementState = MovementState.Standing;
    private bool isRecovering = false;
    private bool isWaitingAutoAction = false;

    public event Action OnAction;

    private void Start()
    {
        playerTransform = transform;
        RecoverStamina();
        OnAction += PerformAction;
    }

    private void Update()
    {
        if (!isWaitingAutoAction)
        {
            isWaitingAutoAction = true;
            float randomTime = UnityEngine.Random.Range(0.1f, 2f);
            // Invoke Action
            Invoke(nameof(InvokeAction), randomTime);
        }
    }

    private void InvokeAction()
    {
        OnAction?.Invoke();
    }

    private void PerformAction()
    {
        HandleAction();
        isWaitingAutoAction = false;
    }

    public void HandleAction()
    {
        if (isRecovering)
            return;

        UseStamina();
        HandleMovementState();
    }

    private IEnumerator RecoverStaminaCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (stamina < 100)
                stamina += staminaRecovery;

            stamina = Mathf.Clamp(stamina, 0, 100);
        }
    }

    private void RecoverStamina()
    {
        StartCoroutine(RecoverStaminaCoroutine());
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
        float moveSpeed = (currentMovementState == MovementState.Walking) ? speed : speed * 2;
        MovePlayer(moveSpeed);
    }

    private void MovePlayer(float currentSpeed)
    {
        Vector3 targetPosition = playerTransform.position + new Vector3(currentSpeed * Time.deltaTime, 0, 0);
        playerTransform.DOMove(targetPosition, 0.5f);
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

    public void SetPlayerColor(GameplayManagerSingleMode.PLAYER_COLOR color)
    {
        Renderer playerRenderer = GetComponentInChildren<Renderer>();
        playerRenderer.material.color = GetColor(color);
    }

    private Color GetColor(GameplayManagerSingleMode.PLAYER_COLOR color)
    {
        switch (color)
        {
            case GameplayManagerSingleMode.PLAYER_COLOR.RED:
                return Color.red;
            case GameplayManagerSingleMode.PLAYER_COLOR.BLUE:
                return Color.blue;
            case GameplayManagerSingleMode.PLAYER_COLOR.GREEN:
                return Color.green;
            case GameplayManagerSingleMode.PLAYER_COLOR.YELLOW:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    public float CurrentPercentageStamina()
    {
        return stamina / 100;
    }
}
