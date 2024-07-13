using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

public class DailyRewardUIItem : MonoBehaviour
{
    [Header("UI REREFERENCES")]
    public Image imgIcon;
    public TextMeshProUGUI txtQuantity;
    public TextMeshProUGUI txtDayReward;
    public Button btnClaimReward;
    [Header("OBJECT REREFERENCES")]
    public GameObject objClaimedBG;
    public TweeningAnimationZoomIn tweenAnimClaimingTick;

    private DailyRewardItem m_Data;

    [SerializeField][ReadOnly]
    private DailyRewardClaimableStatus m_ClaimableStatus;

    public void Refresh(DailyRewardItem data, DailyRewardClaimableStatus claimableStatus)
    {
        this.m_Data = data;
        this.m_ClaimableStatus = claimableStatus;
        RefreshUI();
    }

    private void RefreshUI()
    {
        SetText_Quantity();
        SetText_DayReward();

        ToggleObj_ClaimedBG();
        ToggleInteractable_ButtonClaimReward();

        OnClaimingAnimation();
    }
    private void SetText_DayReward()
    {
        if (m_Data == null) return;
        if (txtDayReward) txtDayReward.text = "Day" + m_Data.rewadDay;
    }
    private void SetText_Quantity()
    {
        if (m_Data == null) return;
        if (txtQuantity) txtQuantity.text = "x" + m_Data.quantity;
    }
    private void ToggleInteractable_ButtonClaimReward()
    {
        if (btnClaimReward) btnClaimReward.interactable = m_ClaimableStatus == DailyRewardClaimableStatus.Claimable;
    }
    private void ToggleObj_ClaimedBG()
    {
        if (objClaimedBG) objClaimedBG.SetActive(m_ClaimableStatus != DailyRewardClaimableStatus.Claimable);
    }

    public void OnClaimingAnimation()
    {
        if (m_ClaimableStatus != DailyRewardClaimableStatus.Claimed) return;
        if(tweenAnimClaimingTick)
        {
            tweenAnimClaimingTick.PlayAnimation();
        }
    }
    #region ____BUTTON ONCLICKS____
    public void OnClick_ClaimReward()
    {
        if (m_Data == null) return;
        if (m_ClaimableStatus != DailyRewardClaimableStatus.Claimable) return;
        DailyRewardManager.Instance.ClaimDailyRewardItem(m_Data, (success) =>
        {
            if(success)
            {
                Refresh(m_Data, DailyRewardClaimableStatus.Claimed);
            }else
            {
                Debug.LogError("DAILY REWARD: cannot claim reward!");
            }
        });
        
    }
    #endregion
}
