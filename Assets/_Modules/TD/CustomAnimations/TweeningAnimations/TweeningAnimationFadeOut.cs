using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweeningAnimationFadeOut : TweeningAnimation
{
    public AnimationFadeMethod animationFadeMethod;
    public float fadeOutValue;
    public GameObject objectFadeOut;

    protected CanvasGroup m_CanvasGroup;
    protected Image m_Image;
    //protected SkeletonGraphic m_SkeletonGraphic;


    public TweeningAnimationFadeOut(
           float duration,
           Ease ease,
           float fadeOutValue,
           ref GameObject objectFadeOut,
           AnimationFadeMethod animationFadeMethod) : base(duration, ease, TweeningAnimationType.FADE_OUT)
    {
        this.fadeOutValue = fadeOutValue;
        this.objectFadeOut = objectFadeOut;
        this.animationFadeMethod = animationFadeMethod;
    }

    public override void Awake()
    {
        base.Awake();
    }
    public override void OnInit()
    {
        base.OnInit();
        if (objectFadeOut == null) objectFadeOut = this.gameObject;
        this.tweeningAnimationType = TweeningAnimationType.FADE_OUT;

        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (objectFadeOut.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
                {
                    m_CanvasGroup = canvasGroup;
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (objectFadeOut.TryGetComponent<Image>(out Image img))
                {
                    m_Image = img;
                }
                break;
            /*case AnimationFadeMethod.SKELETON_GRAPHIC:
                if (objectFadeOut.TryGetComponent<SkeletonGraphic>(out SkeletonGraphic ske))

                {
                    m_SkeletonGraphic = ske;
                }
                break;*/
        }
    }
    public override Tween GetTweenAnimation()
    {
        if (objectFadeOut == null) return null;
        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (m_CanvasGroup)
                {
                    m_TweenAnimation = m_CanvasGroup.DOFade(fadeOutValue, tweenDuration)
                                                    .SetEase(easeType);
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (m_Image)
                {
                    m_TweenAnimation = m_Image.DOFade(fadeOutValue, tweenDuration)
                                              .SetEase(easeType);
                }
                break;
           /* case AnimationFadeMethod.SKELETON_GRAPHIC:
                if (m_SkeletonGraphic)
                {
                    m_TweenAnimation = m_SkeletonGraphic.DOFade(fadeOutValue, tweenDuration)
                                                        .SetEase(easeType);
                }
                break;*/
        }

        RegisterOnStartAndOnCompleteCallbacks();
        return m_TweenAnimation;
    }

    public override void SetupFirstState()
    {
        base.SetupFirstState();

        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (m_CanvasGroup)
                {
                    m_CanvasGroup.alpha = 1;
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (m_Image)
                {
                    Color color = m_Image.color;
                    color.a = 1;
                    m_Image.color = color;
                }
                break;
            /*case AnimationFadeMethod.SKELETON_GRAPHIC:
                if (m_SkeletonGraphic)
                {
                    Color tmp = m_SkeletonGraphic.color;
                    tmp.a = 1;
                    m_SkeletonGraphic.color = tmp;
                }
                break;*/
        }
    }

    
}
