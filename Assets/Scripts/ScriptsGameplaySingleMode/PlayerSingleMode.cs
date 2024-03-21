using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSingleMode : MonoBehaviour
{
    // Mỗi nhân vật có 100 điểm thể lực
    // Mỗi lần click mất 5 điểm thể lực
    // Có 3 trạng thái di chuyển
    // Đứng thở: khi thể lực bằng 0
    // đi bộ: Khi thể lực dưới 50đ
    // Chạy: Khi thể lực trên 50đ
    // Mỗi giây hồi 10 điểm thể lực
    // Khi thể lực bằng 0. Nhân vật mất 2s để đứng lại để thở.


    // transform of the player base on x
    [SerializeField] private Transform playerTransform;
    // speed of the player
    [SerializeField] private float speed = 10f;

    // Mỗi nhân vật có 100 điểm thể lực
    [SerializeField] private float stamina = 100;
    // Mỗi lần click mất 5 điểm thể lực
    [SerializeField] private float staminaCost = 5;
    // Có 3 trạng thái di chuyển
    // Đứng thở: khi thể lực bằng 0
    // đi bộ: Khi thể lực dưới 50đ
    // Chạy: Khi thể lực trên 50đ
    private enum MOVEMENT_STATE { STANDING, WALKING, RUNNING }
    [SerializeField] private MOVEMENT_STATE currentMovementState = MOVEMENT_STATE.STANDING;
    // Mỗi giây hồi 10 điểm thể lực
    [SerializeField] private float staminaRecovery = 10;
    // Khi thể lực bằng 0. Nhân vật mất 2s để đứng lại để thở.
    [SerializeField] private float timeToRecover = 2f;
    [SerializeField] private bool isRecovering = false;


    void Start()
    {
        playerTransform = GetComponent<Transform>();
        // recover stamina
        RecoverStamina();
    }

    void Update() {
        // get input touch


        if (Input.GetMouseButtonDown(0))
        {
            if (isRecovering)
            {
                return;
            }
            Debug.Log("Click");
            OnUseStamina();
            HandleMovementState();
        }
    }

    private void RecoverStamina()
    {
        StartCoroutine(RecoverStaminaCoroutine());
    }

    IEnumerator RecoverStaminaCoroutine()
    {
        while (true)
        {
            Debug.Log("Recovering stamina");
            yield return new WaitForSeconds(1);
            if (stamina < 100)
            {
                stamina += staminaRecovery;
            }
            if (stamina > 100)
            {
                stamina = 100;
            }
        }
    }

    private MOVEMENT_STATE GetMovementState()
    {
        if (stamina <= 0)
        {
            return MOVEMENT_STATE.STANDING;
        }
        else if (stamina < 50)
        {
            return MOVEMENT_STATE.WALKING;
        }
        else
        {
            return MOVEMENT_STATE.RUNNING;
        }
    }

    private void HandleMovementState()
    {
        currentMovementState = GetMovementState();
        switch (currentMovementState)
        {
            case MOVEMENT_STATE.STANDING:
                break;
            case MOVEMENT_STATE.WALKING:
                // move the player
                playerTransform.position += new Vector3(speed * Time.deltaTime, 0, 0);
                break;
            case MOVEMENT_STATE.RUNNING:
                // move the player
                playerTransform.position += new Vector3(speed * Time.deltaTime * 2, 0, 0);
                break;
        }
    }

    private void OnUseStamina()
    {
        if (stamina > 0)
        {
            stamina -= staminaCost;
        }
        if (stamina <= 0)
        {
            isRecovering = true;
            stamina = 0;
            Invoke("OnRecoverDone", timeToRecover);
        }
    }
    private void OnRecoverDone()
    {
        isRecovering = false;
    }
}
