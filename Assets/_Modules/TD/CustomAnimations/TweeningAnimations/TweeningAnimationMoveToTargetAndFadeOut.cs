using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TD.Utilities;
using TMPro;

public class TweeningAnimationMoveToTargetAndFadeOut : TweeningAnimationMoveToTarget
{
    [Header("MOVE AND FADE SETTINGS")]
    public AnimationFadeMethod animationFadeMethod;
    public SequentialAnimationType sequentialAnimationType;
    public float tweenInterval = 0.2f;
    public float fadeOutValue;

    protected CanvasGroup m_CanvasGroup;
    protected Image m_Image;
    protected TextMeshProUGUI m_TextMeshPro;


    public TweeningAnimationMoveToTargetAndFadeOut(
            float duration,
            Ease ease,
            Transform targetTransform,
            ref Transform objectTransform,
            bool MoveByRect,
            AnimationMoveType moveType,
            AnimationFadeMethod fadeMethod,
            float fadeOutValue,
            float interval,
            SequentialAnimationType sequentialAnimationType) : base(duration, ease, targetTransform, ref objectTransform, MoveByRect, moveType)
    {
        this.fadeOutValue = fadeOutValue;
        this.animationFadeMethod = fadeMethod;
        this.sequentialAnimationType = sequentialAnimationType;
        this.tweenInterval = interval;
    }

    public override void OnInit()
    {
        if (objectTransform == null) objectTransform = this.transform;
        this.tweeningAnimationType = TweeningAnimationType.MOVE_TO_TARGET_FADE_OUT;
        SetCachedPosition();
    }
    public override void SetupFirstState()
    {
        base.SetupFirstState();
        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (objectTransform.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
                {
                    m_CanvasGroup = canvasGroup;
                    canvasGroup.alpha = 1;
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (objectTransform.TryGetComponent<Image>(out Image img))
                {
                    m_Image = img;
                    Color color = img.color;
                    color.a = 1;
                    img.color = color;
                }
                break;
            case AnimationFadeMethod.TEXT_MESH_PRO:
                if (objectTransform.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI txt))

                {
                    m_TextMeshPro = txt;
                    m_TextMeshPro.alpha = 1;
                }
                break;
        }
    }
    public override Tween GetTweenAnimation()
    {
        if (objectTransform == null || targetTransform == null)
            return null;

        Tween tweenMove = GetTweenMoveAnimation();
        Tween tweenFade = GetTweenFadeOutAnimation();
        Sequence sequence = DOTween.Sequence();
        switch(sequentialAnimationType)
        {
            case SequentialAnimationType.Sequential:
                sequence.Append(tweenMove);
                sequence.AppendInterval(tweenInterval);
                sequence.Append(tweenFade);
                break;
            case SequentialAnimationType.Simultanious:
                sequence.Append(tweenMove);
                sequence.Join(tweenFade);
                break;
        }
        m_TweenAnimation = sequence;
        RegisterOnStartAndOnCompleteCallbacks();
        return m_TweenAnimation;
    }

    private Tween GetTweenFadeOutAnimation()
    {
        Tween tweenFade = default;
        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (m_CanvasGroup)
                {
                    tweenFade = m_CanvasGroup.DOFade(fadeOutValue, tweenDuration)
                                             .SetEase(easeType);
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (m_Image)
                {
                    tweenFade = m_Image.DOFade(fadeOutValue, tweenDuration)
                                       .SetEase(easeType);
                }
                break;
            case AnimationFadeMethod.TEXT_MESH_PRO:
                if (m_TextMeshPro)

                {
                    tweenFade = m_TextMeshPro.DOFade(fadeOutValue, tweenDuration)
                                             .SetEase(easeType);
                }
                break;
        }

        return tweenFade;
    }
    private Tween GetTweenMoveAnimation()
    {
        Tween tweenMove = default;
        if (MoveByRect && objectTransform.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            Vector2 targetAnchorPos = VectorUtils.ConvertToRectTransform(targetTransform.GetComponent<RectTransform>(), rect);
            tweenMove = rect.DOAnchorPos(targetAnchorPos, tweenDuration)
                            .SetEase(easeType);
        }
        else
        {
            switch (moveType)
            {
                case AnimationMoveType.LINEAR:
                    // For regular Transform, move it to the target's screen position
                    tweenMove = objectTransform.DOMove(targetTransform.position, tweenDuration)
                                                    .SetEase(easeType);
                    break;
                case AnimationMoveType.CURVE:
                    Vector3[] path = VectorUtils.CalculateBezierCurvePoints(
                                     objectTransform.position,
                                     targetTransform.position,
                                     controlPoint.position, segments);
                    // For RectTransform, move it to the target's screen position
                    tweenMove = objectTransform.DOPath(path, tweenDuration, PathType.CatmullRom);
                    break;
            }
        }

        return tweenMove;
    }
}
