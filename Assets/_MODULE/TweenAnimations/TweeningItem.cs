using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TweeningItem : MonoBehaviour
{
    public enum TweeningType
    {
        DoScale,
        DoMoveFromLeft,
        DoMoveFromRight,
        DoMoveFromTop,
        DoMoveFromBottom,
        DoZoomOut,
        DoZoomOutAndIn,
        DoFlyOut,
        DoFadeIn,
        DoScaleX,
        DoScaleY,
        DoScoreRun,
        None,
        DoFadeOut,
        DoZoomInOutOvertime,
        DoMoveToTarget,
        DoPunchScaleOvertime,
        DoNeonEffect,
        DoFloat,
        DoFadeThenStampIn,
        DoFadeOvertime,
        DoBlink,
        DoFadeInAndFadeOut
    }
    public TweeningType tweeningType = TweeningType.DoScale;
    public bool PlayOnAwake = false;
    public float delay = 0.0001f;

    [SerializeField] protected float fadeTime = 1f;
    [SerializeField] protected Vector2 fromScale = Vector2.one;
    [SerializeField] protected Vector2 toScale = Vector2.one;

    [SerializeField] protected float transitionInterval = 1f;
    [SerializeField] protected bool autoClearTweeningCache = false;
    [SerializeField] protected bool autoClearTweenOnDisable = false;


    [Tooltip("Tick this if you want to set auto scale. This is used for Scaling options only")]
    [SerializeField] bool toCurrentScale = false;
    [SerializeField] protected Ease easeInMode;
    [SerializeField] protected Ease easeOutMode;
    [Header("DOPUNCHSCALEOVERTIME SETTINGS")]
    [SerializeField] protected Vector2 punchingScale = new Vector2(0.1f, 0.1f);
    [SerializeField] protected int vibrato = 10;
    [SerializeField] protected float elasticity = 1;
    [Header("DONEON SETTINGS")]
    [SerializeField] protected float neonStartAlphaVal = 0.5f;
    [Header("DOFLOAT SETTINGS")]
    [SerializeField] protected float floatingHeight = 1f;
    [SerializeField] protected RectTransform floatStartPoint;
    [Header("DOFADE OVERTIME SETTINGS")]
    [SerializeField] protected float fadeInTime = 1f;
    [SerializeField] protected float fadeOutTime = 1f;
    [SerializeField] protected int fadeLoopTime = 1;
    [Header("DOBLINK SETTINGS")]
    [SerializeField] protected GameObject blinkObject;
    [SerializeField] protected Vector2 scaleBlinkModifier = Vector3.zero;
    [SerializeField] protected float blinkInterval = 0.2f;
    [SerializeField] protected float blinkFadeIn = 1;
    [SerializeField] protected float blinkFadeOut = 0;

    private Vector3 originScale = Vector3.one;
    private Vector3 originPosition = Vector3.one;
    public Vector3 OriginScale { get => originScale; set => originScale = value; }
    public Vector3 OriginPosition { get => originPosition; set => originPosition = value; }

    public bool ToCurrentScale { get => toCurrentScale; }
    public bool HasFinishedEffect { get => hasFinishedEffect; set => hasFinishedEffect = value; }
    private bool hasFinishedEffect = false;

    public float FadeTime { get => fadeTime; set => fadeTime = value; }

    public Vector2 ToScale { get => toScale; set => toScale = value; }
    public Vector2 FromScale { get => fromScale; set => fromScale = value; }

    public Ease EaseInMode { get => easeInMode; set => easeInMode = value; }
    public Ease EaseOutMode { get => easeOutMode; set => easeOutMode = value; }

    private Tween iTween;
    public Tween ITween { get => iTween; set => iTween = value; }
    public event Action OnTweenCallbackAction = null;

    private Vector2 blinkObjectOriginScale = Vector2.one;
    protected CanvasGroup blinkObjectCanvasGroup = null;
    protected Vector2 newScaleBlink;

    public virtual void Awake()
    {
        originPosition = transform.position;
        originScale = transform.localScale;
        if (toCurrentScale)
        {
            ToScale = originScale;
        }
        if (tweeningType == TweeningType.DoBlink)
        {
            blinkObjectOriginScale = blinkObject.transform.localScale;
            newScaleBlink = (Vector2)(blinkObjectOriginScale + scaleBlinkModifier);

            blinkObjectCanvasGroup = blinkObject.GetComponent<CanvasGroup>();
        }
    }
    public virtual void OnEnable()
    {
        if (tweeningType == TweeningType.None)
            return;

        Validate();

        if (autoClearTweeningCache)
            ClearCached();

        if (PlayOnAwake)
            StartCoroutine(DOItemAnimation(gameObject, true));
    }
    public virtual void OnDisable()
    {
        StartSleepAnimation();
        if (autoClearTweenOnDisable)
            KillTween();
    }
    #region _____SETUP AND VALIDATE_____
    public virtual void StartSleepAnimation()
    {
        switch (tweeningType)
        {
            case TweeningType.DoFadeInAndFadeOut:
                DOItemAnimation(gameObject, false, TweeningType.DoFadeOut, true);
                break;
        }
    }

    public virtual void Validate()
    {
        GetTransformScaleAndPosition();
    }
    private void GetTransformScaleAndPosition()
    {
        try
        {
            var tmpScale = gameObject.transform.localScale;

            switch (tweeningType)
            {
                case TweeningType.DoFadeThenStampIn:
                    TrySetCanvasGroupAlpha(0);
                    break;
                case TweeningType.DoFadeInAndFadeOut:
                case TweeningType.DoFadeIn:
                    TrySetCanvasGroupAlpha(0);
                    break;
                case TweeningType.DoScaleX:
                    SetLocalScaleX(tmpScale, fromScale.x);
                    break;
                case TweeningType.DoScaleY:
                    SetLocalScaleY(tmpScale, fromScale.y);
                    break;
                case TweeningType.DoZoomOutAndIn:
                case TweeningType.DoScale:
                case TweeningType.DoZoomOut:
                    gameObject.transform.localScale = fromScale;
                    break;
                case TweeningType.DoFadeOvertime:
                    TrySetCanvasGroupAlpha(0);
                    break;
                case TweeningType.DoBlink:
                    SetBlinkObjectVisibility(false);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"TWEENING ITEM: Exception {ex}");
        }
    }

    private void TrySetCanvasGroupAlpha(float alphaValue)
    {
        if (TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = alphaValue;
        }
    }

    private void SetLocalScaleX(Vector3 scale, float value)
    {
        scale.x = value;
        gameObject.transform.localScale = scale;
    }

    private void SetLocalScaleY(Vector3 scale, float value)
    {
        scale.y = value;
        gameObject.transform.localScale = scale;
    }

    private void SetBlinkObjectVisibility(bool isVisible)
    {
        if (blinkObject)
        {
            blinkObject.SetActive(isVisible);
        }

        if (blinkObjectCanvasGroup)
        {
            blinkObjectCanvasGroup.alpha = isVisible ? blinkFadeIn : 0;
        }
    }
    #endregion
    #region _____MAIN METHODS_____
    //=====>TWEENING ANIM<=====//
    public virtual void PauseTween()
    {
        if(iTween != null)
        {
            iTween.Pause();
        }
    }
    public virtual void KillTween()
    {
        if (iTween != null)
        {
            switch(tweeningType)
            {
                case TweeningType.DoBlink:
                    SetBlinkObjectVisibility(false);
                    break;
            }
                
            iTween.Kill();
            iTween = null;
        }
    }
    public virtual void StartAnimationWithCallback(Action callback)
    {
        StartCoroutine(DOItemAnimation(gameObject, false, tweeningType, true, false, callback));
    }
    public virtual IEnumerator DOItemAnimation(GameObject item = null, bool shouldChangesize = false,
                                               TweeningType newType = TweeningType.None,
                                               bool canPlay = false, bool smoothRevert = false,
                                               Action OnCompleteTweeningAction = null)
    {
        if (autoClearTweeningCache) ClearCached();
        if (shouldChangesize) gameObject.transform.localScale = fromScale;

        yield return new WaitForSeconds(delay);
        if (item == null) item = gameObject;
        if(smoothRevert)
        {
            DOScaleBackToOriginalSize(item, OriginScale, fadeTime);
            yield break;
        }
        iTween = GetItemTween(item, originScale, originPosition, newType);
        OnTweenCallbackAction = OnCompleteTweeningAction;
        iTween.OnComplete(() =>
        {
            OnTweenCallbackAction?.Invoke();
            OnTweenCallbackAction = null;
        });

        if (canPlay) iTween.Play();
    }

    public virtual Tween GetItemTween(GameObject item = null, Vector3 originScale = new Vector3(), Vector3 originPosition = new Vector3(), TweeningType newType = TweeningType.None)
    {
        if (item == null) item = gameObject;

        Tween iTween = null;
        TweeningType type = this.tweeningType;
        if (newType != TweeningType.None)
        {
            type = newType;
        }

        switch (type)
        {
            case TweeningType.DoBlink:
                iTween = GetBlinkTween(item);
                break;
            case TweeningType.DoFadeOvertime:
                iTween = GetFadeOvertimeTween(item);
                break;
            case TweeningType.DoFlyOut:
                iTween = GetFlyOutTween(item);
                break;
            case TweeningType.DoFadeThenStampIn:
                iTween = GetFadeThenStampInTween(item);
                break;
            case TweeningType.DoZoomOutAndIn:
                iTween = GetZoomOutAndInTween(item);
                break;
            case TweeningType.DoFadeInAndFadeOut:
            case TweeningType.DoFadeIn:
                iTween = GetFadeInTween(item);
                break;
            case TweeningType.DoFadeOut:
                iTween = GetFadeOutTween(item);
                break;
            case TweeningType.DoScale:
                iTween = GetScaleTween(item);
                break;
            case TweeningType.DoScaleX:
                iTween = GetScaleXTween(item);
                break;
            case TweeningType.DoScaleY:
                iTween = GetScaleYTween(item);
                break;
            case TweeningType.DoZoomInOutOvertime:
                iTween = GetZoomInOutOvertimeTween(item);
                break;
            case TweeningType.DoMoveFromLeft:
                iTween = GetMoveFromLeftTween(item);
                break;
            case TweeningType.DoZoomOut:
                iTween = GetZoomOutTween(item);
                break;
            case TweeningType.DoPunchScaleOvertime:
                iTween = GetPunchScaleOvertimeTween(item);
                break;
            case TweeningType.DoNeonEffect:
                iTween = GetNeonEffectTween(item);
                break;
            case TweeningType.DoFloat:
                iTween = GetFloatTween(item);
                break;
            default:
                break;
        }
        return iTween;
    }
    #region Tween Type Animate
    private Tween GetBlinkTween(GameObject item)
    {
        if (blinkObject == null) return null;
        blinkObject.SetActive(true);

        Sequence sequenceBlink = DOTween.Sequence();
        Sequence sequenceFade = DOTween.Sequence();
        if(blinkObjectCanvasGroup != null)
        {
            blinkObjectCanvasGroup.alpha = blinkFadeIn;
            sequenceFade.Append(blinkObjectCanvasGroup?.DOFade(blinkFadeOut, FadeTime)
                             .OnComplete(() => blinkObjectCanvasGroup.alpha = blinkFadeIn).SetEase(easeInMode));
        }

        Sequence sequenceScale = DOTween.Sequence();
        blinkObject.transform.localScale = blinkObjectOriginScale;
        sequenceScale.Append(blinkObject?.transform.DOScale(newScaleBlink, FadeTime).
                           OnComplete(() => blinkObject.transform.localScale = blinkObjectOriginScale).SetEase(easeInMode));

        sequenceBlink.Append(sequenceFade);
        sequenceBlink.Join(sequenceScale);
        sequenceBlink.AppendInterval(blinkInterval);
        sequenceBlink.SetLoops(-1);

        return sequenceBlink;
    }

    private Tween GetFadeOvertimeTween(GameObject item)
    {
        if (!item.TryGetComponent<CanvasGroup>(out CanvasGroup fadeOverTimeCanvasGroup)) return null;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(fadeOverTimeCanvasGroup.DOFade(1f, FadeTime))
            .AppendInterval(transitionInterval)
            .Append(fadeOverTimeCanvasGroup.DOFade(0f, FadeTime))
            .AppendInterval(transitionInterval)
            .SetLoops(fadeLoopTime);

        return sequence;
    }

    private Tween GetFlyOutTween(GameObject item)
    {
        return item.transform.DOScale(0, fadeTime).SetEase(easeInMode);
    }

    private Tween GetFadeThenStampInTween(GameObject item)
    {
        Tween fadeTween = item.TryGetComponent<CanvasGroup>(out CanvasGroup itemCanvasGroup)
            ? itemCanvasGroup.DOFade(1, FadeTime).SetEase(EaseInMode)
            : null;

        Tween scaleTween = item.transform.DOScale(ToScale, FadeTime).SetEase(EaseInMode);
        Sequence sequenceScale = DOTween.Sequence();
        sequenceScale.Append(fadeTween).AppendInterval(0.3f);

        return sequenceScale;
    }
    private Tween GetZoomOutAndInTween(GameObject item)
    {
        Sequence sequenceZoomOutAndIn = DOTween.Sequence();
        sequenceZoomOutAndIn.Append(item.transform.DOScale(toScale, fadeTime).SetEase(easeInMode));
        sequenceZoomOutAndIn.AppendInterval(transitionInterval);
        sequenceZoomOutAndIn.OnComplete(() => DOScaleBackToOriginalSize(item, fromScale, fadeTime));
        return sequenceZoomOutAndIn;
    }

    private Tween GetFadeInTween(GameObject item)
    {
        if (item.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
        {
            return canvasGroup.DOFade(1, fadeTime).SetEase(easeInMode);
        }
        return null;
    }

    private Tween GetFadeOutTween(GameObject item)
    {
        if (item.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroupFadeOut))
        {
            return canvasGroupFadeOut.DOFade(0, fadeTime).SetEase(easeOutMode)
                .OnComplete(() => ProcessAfterFadingOut(item));
        }
        return null;
    }

    private Tween GetScaleTween(GameObject item)
    {
        return item.transform.DOScale(toScale, fadeTime).SetEase(easeInMode);
    }
    private Tween GetScaleXTween(GameObject item)
    {
        var nextScale = new Vector2(ToScale.x, originScale.y);
        return item.transform.DOScale(nextScale, fadeTime).SetEase(easeInMode);
    }
    private Tween GetScaleYTween(GameObject item)
    {
        var nextScale = new Vector2(originScale.y, ToScale.y);
        return item.transform.DOScale(nextScale, fadeTime).SetEase(easeInMode);
    }

    private Tween GetZoomInOutOvertimeTween(GameObject item)
    {
        Vector2 originalScale = item.transform.localScale;
        Vector2 targetScale = originalScale * toScale;

        Tween zoomInTween = item.transform.DOScale(targetScale, fadeTime / 2).SetEase(easeInMode);
        Tween zoomOutTween = item.transform.DOScale(originalScale, fadeTime / 2).SetEase(easeOutMode).SetDelay(fadeTime / 2);

        Sequence zoomInOutSequence = DOTween.Sequence();
        zoomInOutSequence.Append(zoomInTween).Append(zoomOutTween).SetLoops(-1);

        return zoomInOutSequence;
    }
   
    private Tween GetMoveFromLeftTween(GameObject item)
    {
        item.transform.position = new Vector2(originPosition.x - 1500f, originPosition.y);
        return item.transform.DOLocalMoveX(originPosition.x, fadeTime).SetEase(easeInMode).OnComplete(() => hasFinishedEffect = true);
    }

    private Tween GetZoomOutTween(GameObject item)
    {
        return item.transform.DOScale(ToScale, fadeTime).SetEase(easeInMode).OnComplete(() => hasFinishedEffect = true);
    }

    private Tween GetPunchScaleOvertimeTween(GameObject item)
    {
        item.transform.DORewind();
        return item.transform.DOPunchScale(punchingScale, fadeTime, vibrato, elasticity).SetEase(easeInMode).SetLoops(-1, LoopType.Yoyo);
    }

    private Tween GetNeonEffectTween(GameObject item)
    {
        if (item.TryGetComponent<Image>(out Image img))
        {
            Color newColor = img.color;
            newColor.a = neonStartAlphaVal;
            img.color = newColor;
            return img.DOFade(0, fadeTime).SetLoops(-1, LoopType.Yoyo);
        }
        return null;
    }

    private Tween GetFloatTween(GameObject item)
    {
        if (floatStartPoint == null || !item.TryGetComponent<RectTransform>(out RectTransform itemRect))
        {
            return null;
        }

        Vector2 targetPosition = floatStartPoint.anchoredPosition + Vector2.up * floatingHeight;
        Sequence floatSequence = DOTween.Sequence();

        floatSequence.Append(itemRect.DOAnchorPos(targetPosition, fadeTime * 0.5f).SetEase(Ease.InOutQuad));
        floatSequence.Append(itemRect.DOAnchorPos(itemRect.anchoredPosition, fadeTime * 0.5f).SetEase(Ease.InOutQuad));
        floatSequence.SetLoops(-1, LoopType.Yoyo);

        return floatSequence;
    }
    #endregion
    public virtual void StopTweeningEffect()
    {
        if (iTween.IsActive() && iTween != null)
            iTween.Kill();
    }
    public virtual void ProcessAfterFadingOut(GameObject item)
    {
        item.SetActive(false);
        item.GetComponent<CanvasGroup>().alpha = 1;
    }
    
    public virtual void ResetOriginSize(GameObject gameObject)
    {
        gameObject.transform.localScale = fromScale;
        Debug.Log("TWEENING ITEM: reset to scale " + gameObject.transform.localScale);
    }
    public virtual IEnumerator SetActiveAftertime(GameObject gameObject, float time, bool isActive)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(isActive);
    }
    public virtual void DOScaleBackToOriginalSize(GameObject item, Vector3 originScale, float fadeTime)
    {
        item.transform.DOScale(originScale, fadeTime).SetEase(easeOutMode);
        //Clearing cache
        if (autoClearTweeningCache)
            ClearCached();
    }
    public void ClearCached()
    {
        DOTween.ClearCachedTweens();
        DOTween.Kill(this);
    }
    #endregion
}
