using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweeningAnimationZoomOut : TweeningAnimation
{
    public Vector3 zoomOutValue;
    public Transform objectTransform;

    
    public override void OnInit()
    {
        base.OnInit();
        if (objectTransform == null) objectTransform = this.transform;
        this.tweeningAnimationType = TweeningAnimationType.ZOOM_OUT;
    }

    public override Tween GetTweenAnimation()
    {
        if (objectTransform == null) return null;

        m_TweenAnimation = objectTransform.DOScale(zoomOutValue, tweenDuration)
                                          .SetEase(easeType)
                                          .OnStart(()=>OnStartAnimationEvent?.Invoke())
                                          .OnComplete(()=>OnCompleteAnimationEvent?.Invoke());
        return m_TweenAnimation;
    }

    public override void SetupFirstState()
    {
        base.SetupFirstState();
        objectTransform.localScale = Vector3.one;
    }
    public TweeningAnimationZoomOut(float duration, Ease ease, Vector3 zoomOutValue, ref Transform objectTransform) : base(duration, ease, TweeningAnimationType.ZOOM_OUT)
    {
        this.zoomOutValue = zoomOutValue;
        this.objectTransform = objectTransform;
    }
}
