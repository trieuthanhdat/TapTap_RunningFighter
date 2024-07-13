using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TweeningAnimationBlinking : TweeningAnimation
{
    public enum BlinkMethod
    {
        NONE,   
        IMAGE,
        CANVAS_GROUP,
        SPRITE_RENDERER,
        TEXT_MESH_PRO,
        TEXT_AMA,
        TEXT_BUILT_IN
    }
    [Header("BLINK SETTINGS")]
    public BlinkMethod blinkMethod;
    public GameObject goAnimationTarget;
    public float fadeEndValue = 0.5f;
    public int loopTime = 1;
    public bool AnimateColor = true;
    public Color colorBlink = Color.white;

    private Image m_Image;
    private SpriteRenderer m_SpriteRenderer;
    private CanvasGroup m_CanvasGroup;
    private TextMeshProUGUI m_TextMeshPro;
    private Text m_TextBuiltIn;

    private float m_InitialAlphaValue = 1;
    private Color m_InitialColor = Color.white;

    public TweeningAnimationBlinking
    (
        float duration,
        Ease ease,
        TweeningAnimationType type,
        BlinkMethod blinkMethod
    ) : base(duration, ease, type)
    {
        this.blinkMethod = blinkMethod;
    }
    public override void OnInit()
    {
        base.OnInit();
        if (goAnimationTarget == null) goAnimationTarget = this.gameObject;
        this.tweeningAnimationType = TweeningAnimationType.BLINKING;

        switch (blinkMethod)
        {
            case BlinkMethod.IMAGE:
                m_Image = goAnimationTarget.GetComponent<Image>();
                break;
            case BlinkMethod.CANVAS_GROUP:
                m_CanvasGroup = goAnimationTarget.GetComponent<CanvasGroup>();
                break;
            case BlinkMethod.SPRITE_RENDERER:
                m_SpriteRenderer = goAnimationTarget.GetComponent<SpriteRenderer>();
                break;
            case BlinkMethod.TEXT_MESH_PRO:
                m_TextMeshPro = goAnimationTarget.GetComponent<TextMeshProUGUI>();
                break;
            case BlinkMethod.TEXT_BUILT_IN:
                m_TextBuiltIn = goAnimationTarget.GetComponent<Text>();
                break;
        }
        CacheInitialValues();
    }
    public void Reset()
    {
        Tween tween = default;
        switch (blinkMethod)
        {
            case BlinkMethod.IMAGE:
                tween = GetImageBlinkTween(m_InitialAlphaValue, m_InitialColor, tweenDuration / 2);
                break;
            case BlinkMethod.CANVAS_GROUP:
                tween = GetCanvasGroupTween(m_InitialAlphaValue, tweenDuration / 2);
                break;
            case BlinkMethod.SPRITE_RENDERER:
                tween = GetSpriteRendererBlinkTween(m_InitialAlphaValue, m_InitialColor, tweenDuration/2);
                break;
            case BlinkMethod.TEXT_MESH_PRO:
                tween = GetTextBlinkTween(m_InitialAlphaValue, m_InitialColor, tweenDuration/2);
                break;
            case BlinkMethod.TEXT_BUILT_IN:
                tween = GetTextBuiltInBlinkTween(m_InitialAlphaValue, m_InitialColor, tweenDuration / 2);
                break;
        }
        m_TweenAnimation = tween;
        m_TweenAnimation.Play();
    }
    protected void CacheInitialValues()
    {
        switch (blinkMethod)
        {
            case BlinkMethod.IMAGE:
                if (m_Image)
                {
                    m_InitialAlphaValue = m_Image.color.a;
                    m_InitialColor = m_Image.color;
                }
                break;
            case BlinkMethod.CANVAS_GROUP:
                if (m_CanvasGroup)
                {
                    m_InitialAlphaValue = m_CanvasGroup.alpha;
                }
                break;
            case BlinkMethod.SPRITE_RENDERER:
                if (m_SpriteRenderer)
                {
                    m_InitialAlphaValue = m_SpriteRenderer.color.a;
                    m_InitialColor = m_SpriteRenderer.color;
                }
                break;
            case BlinkMethod.TEXT_MESH_PRO:
                if (m_TextMeshPro)
                {
                    m_InitialAlphaValue = m_TextMeshPro.color.a;
                    m_InitialColor = m_TextMeshPro.color;
                }
                break;
            case BlinkMethod.TEXT_BUILT_IN:
                if (m_TextBuiltIn)
                {
                    m_InitialAlphaValue = m_TextBuiltIn.color.a;
                    m_InitialColor = m_TextBuiltIn.color;
                }
                break;
        }
    }
    public void SetInitialColor(Color newColor)
    {
        m_InitialColor = newColor;
    }
    public override Tween GetTweenAnimation()
    {
        switch (blinkMethod)
        {
            case BlinkMethod.IMAGE:
                m_TweenAnimation = GetImageBlinkTween(fadeEndValue, colorBlink, tweenDuration);
                break;
            case BlinkMethod.CANVAS_GROUP:
                m_TweenAnimation = GetCanvasGroupTween(fadeEndValue, tweenDuration);
                break;
            case BlinkMethod.SPRITE_RENDERER:
                m_TweenAnimation = GetSpriteRendererBlinkTween(fadeEndValue, colorBlink, tweenDuration);
                break;
            case BlinkMethod.TEXT_MESH_PRO:
                m_TweenAnimation = GetTextBlinkTween(fadeEndValue, colorBlink, tweenDuration);
                break;
            case BlinkMethod.TEXT_BUILT_IN:
                m_TweenAnimation = GetTextBuiltInBlinkTween(fadeEndValue, colorBlink, tweenDuration);
                break;
        }
        m_TweenAnimation.SetEase(easeType).SetLoops(loopTime);
        RegisterOnStartAndOnCompleteCallbacks();

        return m_TweenAnimation;
    }
    private Tween GetCanvasGroupTween(float fadeEndVal, float tweenDuration)
    {
        if (m_CanvasGroup == null) return null;
        return m_CanvasGroup.DOFade(fadeEndVal, tweenDuration);
    }
    private Tween GetImageBlinkTween(float fadeEndVal, Color color, float tweenDuration)
    {
        if (m_Image == null) return null;

        Sequence sequenceBlink = DOTween.Sequence();
        if (AnimateColor)
        {
            sequenceBlink.Join(m_Image.DOColor(color, tweenDuration));
        }
        sequenceBlink.Join(m_Image.DOFade(fadeEndVal, tweenDuration));
        return sequenceBlink;
    }
    private Tween GetSpriteRendererBlinkTween(float fadeEndVal, Color color, float tweenDuration)
    {
        if (m_SpriteRenderer == null) return null;

        Sequence sequenceBlink = DOTween.Sequence();
        if (AnimateColor)
        {
            sequenceBlink.Join(m_SpriteRenderer.DOColor(color, tweenDuration));
        }
        sequenceBlink.Join(m_SpriteRenderer.DOFade(fadeEndVal, tweenDuration));
        return sequenceBlink;
    }
    private Tween GetTextBlinkTween(float fadeEndVal, Color color, float tweenDuration)
    {
        if (m_TextMeshPro == null) return null;

        Sequence sequenceBlink = DOTween.Sequence();
        if(AnimateColor)
        {
            sequenceBlink.Join(m_TextMeshPro.DOColor(color, tweenDuration));
        }
        sequenceBlink.Join(m_TextMeshPro.DOFade(fadeEndVal, tweenDuration));
        return sequenceBlink;
    }
    
    private Tween GetTextBuiltInBlinkTween(float fadeEndVal, Color color, float tweenDuration)
    {
        if (m_TextBuiltIn == null) return null;

        Sequence sequenceBlink = DOTween.Sequence();
        if (AnimateColor)
        {
            sequenceBlink.Join(m_TextBuiltIn.DOColor(color, tweenDuration));
        }
        sequenceBlink.Join(m_TextBuiltIn.DOFade(fadeEndVal, tweenDuration));
        return sequenceBlink;
    }
}
