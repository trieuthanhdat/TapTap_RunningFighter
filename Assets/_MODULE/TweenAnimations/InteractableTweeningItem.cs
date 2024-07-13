using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableTweeningItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum InteractableTweeningType
    {
        NONE,
        TOUCH_PUNCH_SCALE,
        TOUCH_PUNCH_ROTATION,
        TOUCH_PUNCH_POSITION,
        TOUCH_PUNCH_SCALE_ROTATION_POSITION,
        TOUCH_PUNCH_SCALE_AND_SHOW_TWEENINGOBJECT
    }
    [SerializeField] protected bool PlayOnce;
    [SerializeField] protected bool LockInteraction;
    [SerializeField] protected bool InitOnStart = true;
    [SerializeField] protected bool EnableScaleUpIntro = false;

    [Header("TWEENING SETTINGS")]
    [SerializeField] protected InteractableTweeningType interactableTweeningType;
    [SerializeField] protected float tweenTimeForEachInteraction = 0.5f;
    [SerializeField] protected float delayTimeBeforeEachInteraction = 0.1f;
    [SerializeField] protected float timeAllowUnitlNextInteraction = 0.1f;
    [SerializeField] protected Ease easeIn = Ease.Linear;
    [SerializeField] protected Ease easeOut = Ease.Linear;
    [SerializeField] protected bool playEffectInstantly = false;
    [SerializeField] protected float eslaticity = 1;
    [SerializeField] protected int vibrato = 10;
    [Header("PUNCH_SCALE SETTINGS")]
    [SerializeField] protected Vector2 scaleToPunch = new Vector2(0.1f, 0.1f);
    [Header("PUNCH_ROTATION SETTINGS")]
    [SerializeField] protected Vector3 rotationToPunch = Vector3.one;
    [Header("PUNCH_POSITION SETTINGS")]
    [SerializeField] protected Vector2 positionToPucnh = Vector3.zero;
    [Header("PUNCH_SCALE AND SHOW TWEENING OBJECT SETTINGS")]
    [SerializeField] protected TweeningItem tweenTarget;
    [Tooltip("Set -1 for unlimited punch time, set 0 for no repeat")]
    [SerializeField] protected int repeatPunchTime = 1;

    protected Transform myTransform;
    protected GameObject myObject;
    protected RectTransform myRectTransform;
    protected CanvasGroup myCanvasGroup;

    protected Vector2 initialScale;
    protected Vector2 initialPosition;
    protected Quaternion initialRotation;
    protected Vector2 initialRectScale;
    protected Vector2 initialRectPosition;
    protected Quaternion initialRectRotation;

    protected bool isInit = false;
    public bool IsInited => isInit;

    protected bool canInteract = true;
    protected bool hasPlayedOnce = false;

    private float timeCountNextInteraction = 0;

    public virtual void Awake()
    {
        myObject = this.gameObject;
        myTransform = this.transform;
        timeCountNextInteraction = timeAllowUnitlNextInteraction;
        if (gameObject.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            myRectTransform = rect;
        }
        if (gameObject.TryGetComponent<CanvasGroup>(out CanvasGroup cvg))
        {
            myCanvasGroup = cvg;
        }
            
    }
    public virtual void Update()
    {
        timeCountNextInteraction += Time.deltaTime;
        if(timeCountNextInteraction >= timeAllowUnitlNextInteraction)
        {
            canInteract = true;
        }
        else
        {
            canInteract = false;
        }
    }

    public virtual void Start()
    {
        if (InitOnStart) FirstSetup();
        if (EnableScaleUpIntro) ScaleUpIntro();
    }
    public virtual void ScaleUpIntro()
    {
        if (myRectTransform)
        {
            myRectTransform.localScale = Vector3.zero;
            myRectTransform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
        }
        else
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(initialScale, 1f).SetEase(Ease.OutBounce);
        }
    }
    public virtual void Reset()
    {
        hasPlayedOnce = false;
    }
    public virtual void FirstSetup()
    {
        initialPosition = myTransform.position;
        initialScale = myTransform.localScale;
        initialRotation = myTransform.rotation;
        if(myRectTransform)
        {
            initialRectScale = myRectTransform.localScale;
            initialRectPosition = myRectTransform.anchoredPosition;
            initialRectRotation = myRectTransform.rotation;
        }
        isInit = true;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (LockInteraction) return;
        if (PlayOnce && hasPlayedOnce) return;
        if (!isInit) return;
        if (!canInteract) return;
        timeCountNextInteraction = 0;
        if (playEffectInstantly)
        {
            InstantTweeningEffect();
        }
        else
        {
            StartCoroutine(CoroutineTweeningEff());
        }

    }
    private void InstantTweeningEffect()
    {
        DOTweeningAnimation();
    }
    private IEnumerator CoroutineTweeningEff()
    {
        yield return new WaitForSeconds(delayTimeBeforeEachInteraction);
        DOTweeningAnimation();
    }

    public virtual void DOTweeningAnimation()
    {
        switch (interactableTweeningType)
        {
            case InteractableTweeningType.TOUCH_PUNCH_SCALE:
                HandleTOUCH_PUNCH_SCALE();
                break;
            case InteractableTweeningType.TOUCH_PUNCH_ROTATION:
                HandleTOUCH_PUNCH_ROTATION();
                break;
            case InteractableTweeningType.TOUCH_PUNCH_POSITION:
                HandleTOUCH_PUNCH_POSITION();
                break;
            case InteractableTweeningType.TOUCH_PUNCH_SCALE_ROTATION_POSITION:
                HandleTOUCH_PUNCH_SCALE_ROTATION_POSITION();
                break;
            case InteractableTweeningType.TOUCH_PUNCH_SCALE_AND_SHOW_TWEENINGOBJECT:
                HandleTOUCH_PUNCH_SCALE_AND_SHOW_TWEENINGOBJECT();
                break;
        }

        hasPlayedOnce = true;
    }

    public virtual void HandleTOUCH_PUNCH_SCALE_ROTATION_POSITION()
    {
        if (myRectTransform != null)
        {
            myRectTransform.DORewind();
            Sequence sequenceRectTrans = DOTween.Sequence();
            sequenceRectTrans.Join(myRectTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceRectTrans.Join(myRectTransform.DOPunchRotation(rotationToPunch, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceRectTrans.Join(myRectTransform.DOPunchPosition(positionToPucnh, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceRectTrans.OnComplete(() =>
            {
                DOReset(true, true, true);
            });
        }
        else
        {
            Sequence sequenceTransform = DOTween.Sequence();
            myTransform.DORewind();
            sequenceTransform.Join(myTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceTransform.Join(myTransform.DOPunchRotation(rotationToPunch, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceTransform.Join(myTransform.DOPunchPosition(positionToPucnh, tweenTimeForEachInteraction, vibrato, eslaticity).SetEase(easeIn).SetLoops(repeatPunchTime));
            sequenceTransform.OnComplete(() =>
            {
                DOReset(true, true, true);
            });
        }
    }

    public void DOReset(bool resetScale = false, bool resetRotation = false, bool resetPosition = false)
    {
        if (myRectTransform != null)
        {
            if (resetScale) myRectTransform.DOScale(initialRectScale, tweenTimeForEachInteraction).SetEase(easeOut);
            if (resetRotation) myRectTransform.DORotate(initialRectRotation.eulerAngles, tweenTimeForEachInteraction).SetEase(easeOut);
            if (resetPosition) myRectTransform.DOAnchorPos(initialRectPosition, tweenTimeForEachInteraction).SetEase(easeOut);
        }
        else
        {
            if (resetScale) myTransform.DOScale(initialScale, tweenTimeForEachInteraction).SetEase(easeOut);
            if (resetRotation) myTransform.DORotate(initialRotation.eulerAngles, tweenTimeForEachInteraction).SetEase(easeOut);
            if (resetPosition) myRectTransform.DOMove(initialPosition, tweenTimeForEachInteraction).SetEase(easeOut);
        }
    }

    public virtual void HandleTOUCH_PUNCH_POSITION()
    {
            
        if (myRectTransform != null)
        {
            myRectTransform.DORewind();
            myRectTransform.DOPunchPosition(positionToPucnh, tweenTimeForEachInteraction, vibrato, eslaticity)
                            .SetEase(easeIn)
                            .SetLoops(repeatPunchTime)
                            .OnComplete(() =>
                            {
                                DOReset(false, false, true);
                            });
        }
        else
        {
            myTransform.DORewind();
            myTransform.DOPunchPosition(positionToPucnh, tweenTimeForEachInteraction, vibrato, eslaticity)
                        .SetEase(easeIn)
                        .SetLoops(repeatPunchTime)
                        .OnComplete(() =>
                        {
                            DOReset(false, false, true);
                        });
        }
    }

    public virtual void HandleTOUCH_PUNCH_ROTATION()
    {
            
        if (myRectTransform != null)
        {
            myRectTransform.DORewind();
            myRectTransform.DOPunchRotation(rotationToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                            .SetEase(easeIn)
                            .SetLoops(repeatPunchTime)
                            .OnComplete(() =>
                            {
                                DOReset(false, true, false);
                            });
        }
        else
        {
            myTransform.DORewind();
            myTransform.DOPunchRotation(rotationToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                        .SetEase(easeIn)
                        .SetLoops(repeatPunchTime)
                        .OnComplete(() =>
                        {
                            DOReset(false, true, false);
                        });
        }
    }

    public virtual void HandleTOUCH_PUNCH_SCALE()
    {
            
        if (myRectTransform != null)
        {
            myRectTransform.DORewind();
            myRectTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                            .SetEase(easeIn)
                            .SetLoops(repeatPunchTime)
                            .OnComplete(() =>
                            {
                                DOReset(true, false, false);
                            });
        }
        else
        {
            myTransform.DORewind();
            myTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                        .SetEase(easeIn)
                        .SetLoops(repeatPunchTime)
                        .OnComplete(() =>
                        {
                            DOReset(true, false, false);
                        });
        }
    }
    public virtual void HandleTOUCH_PUNCH_SCALE_AND_SHOW_TWEENINGOBJECT()
    {
            
        if (myRectTransform != null)
        {
            myRectTransform.DORewind();
            myRectTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                            .SetEase(easeIn)
                            .SetLoops(repeatPunchTime)
                            .OnComplete(() =>
                            {
                                DOReset(true, false, false);
                            });
        }
        else
        {
            myTransform.DORewind();
            myTransform.DOPunchScale(scaleToPunch, tweenTimeForEachInteraction, vibrato, eslaticity)
                        .SetEase(easeIn)
                        .SetLoops(repeatPunchTime)
                        .OnComplete(() =>
                        {
                            DOReset(true, false, false);
                        });
        }
        if(tweenTarget)
        {
            StartCoroutine(tweenTarget.DOItemAnimation(tweenTarget.gameObject, true, tweenTarget.tweeningType, true, true));
        }
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //if (!isInit) return;

    }

        
}
