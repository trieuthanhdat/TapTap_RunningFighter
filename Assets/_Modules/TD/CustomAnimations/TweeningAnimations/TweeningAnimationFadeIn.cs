using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
//using Spine.Unity;

public enum AnimationFadeMethod
{
    CANVAS_GROUP,
    IMAGE,
    TEXT_MESH_PRO,
    SKELETON_GRAPHIC
}
public class TweeningAnimationFadeIn : TweeningAnimation
{
    
    public TweeningAnimationFadeIn(float duration, Ease ease, float fadeInValue, ref GameObject objectFadeIn, AnimationFadeMethod fadeMethod) : base(duration, ease, TweeningAnimationType.FADE_IN)
    {
        this.fadeInValue = fadeInValue;
        this.objectFadeIn = objectFadeIn;
        this.animationFadeMethod = fadeMethod;
    }

    public AnimationFadeMethod animationFadeMethod;
    public float fadeInValue;
    public GameObject objectFadeIn;

    protected CanvasGroup m_CanvasGroup;
    protected Image m_Image;
    protected TextMeshProUGUI m_TextMeshPro;
    //protected SkeletonGraphic m_SkeletonGraphic;

    public override void OnInit()
    {
        base.OnInit();
        if (objectFadeIn == null) objectFadeIn = this.gameObject;
        this.tweeningAnimationType = TweeningAnimationType.FADE_IN;
    }
    public override void SetupFirstState()
    {
        base.SetupFirstState();
        switch(animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (objectFadeIn.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
                {
                    m_CanvasGroup = canvasGroup;
                    canvasGroup.alpha = 0;
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (objectFadeIn.TryGetComponent<Image>(out Image img))
                {
                    m_Image = img;
                    Color color = img.color;
                    color.a = 0;
                    img.color = color;
                }
                break;
            case AnimationFadeMethod.TEXT_MESH_PRO:
                if (objectFadeIn.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI txt))

                {
                    m_TextMeshPro = txt;
                    m_TextMeshPro.alpha = 0;
                }
                break;
            /*case AnimationFadeMethod.SKELETON_GRAPHIC:
                if (objectFadeIn.TryGetComponent<SkeletonGraphic>(out SkeletonGraphic ske))

                {
                    m_SkeletonGraphic = ske;
                    Color tmp = m_SkeletonGraphic.color;
                    tmp.a = 0;
                    m_SkeletonGraphic.color = tmp;
                }
                break;*/

        }
    }

    public override Tween GetTweenAnimation()
    {
        if (objectFadeIn == null) return null;
        switch (animationFadeMethod)
        {
            case AnimationFadeMethod.CANVAS_GROUP:
                if (m_CanvasGroup)
                {
                    m_TweenAnimation = m_CanvasGroup.DOFade(fadeInValue, tweenDuration)
                                                    .SetEase(easeType);
                }
                break;
            case AnimationFadeMethod.IMAGE:
                if (m_Image)
                {
                    m_TweenAnimation = m_Image.DOFade(fadeInValue, tweenDuration)
                                              .SetEase(easeType);
                }
                break;
            case AnimationFadeMethod.TEXT_MESH_PRO:
                if (m_Image)
                {
                    m_TweenAnimation = m_TextMeshPro.DOFade(fadeInValue, tweenDuration)
                                             .SetEase(easeType);
                }
                break;
            /*case AnimationFadeMethod.SKELETON_GRAPHIC:
                if (m_SkeletonGraphic)

                {
                    m_SkeletonGraphic.DOFade(fadeInValue, tweenDuration)
                                     .SetEase(easeType);
                }
                break*/;
        }

        RegisterOnStartAndOnCompleteCallbacks();
        return m_TweenAnimation;
    }

    
}
