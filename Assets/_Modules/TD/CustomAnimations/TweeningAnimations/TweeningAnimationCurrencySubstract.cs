using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TD.Utilities.RichTextExtension;
using TMPro;
using UnityEngine;

public class TweeningAnimationCurrencySubstract : TweeningAnimationMoveToTargetAndFadeOut
{
    [SerializeField] private TextMeshProUGUI txtCurrencySubstractAmount;
    [SerializeField] private int m_MaxDigits = 6;
    [SerializeField] private string m_Delimeter = ",";

    public TweeningAnimationCurrencySubstract
    (
        float duration,
        Ease ease,
        Transform targetTransform,
        ref Transform objectTransform,
        bool MoveByRect,
        AnimationMoveType moveType,
        AnimationFadeMethod fadeMethod,
        float fadeOutValue,
        float interval,
        SequentialAnimationType sequentialAnimationType
    ) : base(duration, ease, targetTransform, ref objectTransform, MoveByRect, moveType, fadeMethod, fadeOutValue, interval, sequentialAnimationType)
    {

    }
    public void CheckAndSetInitialialPosition()
    {
        Vector3 newPos = initialTransform ? initialTransform.transform.position :
                                            objectTransform.transform.position;
        SetCachedPosition(newPos);
        objectTransform.transform.position = newPos;
    }
    public void SetCurrencySubStractAmount(int amount)
    {
        if (!txtCurrencySubstractAmount) return;
        txtCurrencySubstractAmount.text = RichTextFormatHelper.RichTextFormat(amount, m_MaxDigits, m_Delimeter);
    }

    public override void OnInit()
    {
        base.OnInit();
        SetCurrencySubStractAmount(0);
    }
}
