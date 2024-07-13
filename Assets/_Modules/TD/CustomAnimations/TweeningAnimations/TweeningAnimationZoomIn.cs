using UnityEngine;
using DG.Tweening;
using System;

public class TweeningAnimationZoomIn : TweeningAnimation
{
    public Vector3 zoomInValue;
    public Transform objectTransform;
    public float zoomOutDelay = 0.5f;
    public override void OnInit()
    {
        base.OnInit();
        if (objectTransform == null) objectTransform = this.transform;
        this.tweeningAnimationType = TweeningAnimationType.ZOOM_IN;
    }
    public override void SetupFirstState()
    {
        base.SetupFirstState();
        objectTransform.localScale = Vector3.zero;
    }
    public void PlayRevertedAnimation(Action OnStartCallback = null, Action OnCompleteCallback = null)
    {
        GetRevertedTweenAnimation()
        .OnStart(   () => OnStartCallback?.Invoke())
        .OnComplete(() => OnCompleteCallback?.Invoke())
        .Play();
    }
    public override Tween GetTweenAnimation()
    {
        if (objectTransform == null) return null;

        m_TweenAnimation = objectTransform.DOScale(zoomInValue, tweenDuration)
                                          .SetEase(easeType);
        RegisterOnStartAndOnCompleteCallbacks();
                                          
        return m_TweenAnimation;
    }
    public Tween GetRevertedTweenAnimation()
    {
        if (objectTransform == null) return null;

        m_TweenAnimation = objectTransform.DOScale(Vector3.zero, tweenDuration)
                                          .SetEase(easeType)
                                          .SetDelay(zoomOutDelay);

        return m_TweenAnimation;
    }

    public TweeningAnimationZoomIn(float duration, Ease ease, Vector3 zoomInValue, ref Transform objectTransform) : base(duration, ease, TweeningAnimationType.ZOOM_IN)
    {
        this.zoomInValue = zoomInValue;
        this.objectTransform = objectTransform;
    }
}