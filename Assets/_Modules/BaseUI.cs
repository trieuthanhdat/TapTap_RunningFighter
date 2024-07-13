using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;
using Observer;

public class BaseUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [Tooltip("type popup")]
    public bool isPopup;
    [Tooltip("On off control with on off popup")]
    [SerializeField] bool isChangeControl;
    [Tooltip("show popup while begin")]
    [SerializeField] bool isShow;
    [Tooltip("auto hide popup")]
    [SerializeField] bool isAutoHide;
    [Tooltip("sound while open popup")]
    [SerializeField] bool soundOpen;
    [Tooltip("sound while hide popup")]
    [SerializeField] bool soundClose;
    [Tooltip("assign object if want anim scale")]
    [SerializeField] GameObject PopupShow;
    [SerializeField] UnityEvent[] eventShow;
    [SerializeField] UnityEvent[] eventHide;
    Coroutine coroutine_autoHide;
    Vector3 originScalePopup;
    [Header("anim popup slide")]
    [SerializeField] GameObject popupSlide;
    [SerializeField] float fromY, toY;

    public static event Action<BaseUI> OnPopupShown;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (PopupShow != null)
        {
            originScalePopup = PopupShow.transform.localScale;
        }

    }
    private void OnEnable()
    {
        BaseUI.OnPopupShown += BaseUI_OnPopupShown;
        EventDispatcher.Instance?.RegisterListener(EventID.OnChangeScene, OnChangeScene);
    }
    private void OnDisable()
    {
        BaseUI.OnPopupShown -= BaseUI_OnPopupShown;
        EventDispatcher.Instance?.RemoveListener(EventID.OnChangeScene, OnChangeScene);
    }
    private void OnChangeScene(object obj)
    {
        if (isShow)
            HidePopup();
    }
    private void BaseUI_OnPopupShown(BaseUI objShown)
    {
        if (this.gameObject == objShown) return;
        if (!this.isPopup || !objShown.isPopup) return;
        if (isShow)
            HidePopup();
    }

    private void Start()
    {
        if (!isShow)
            HidePopup();
        else
            ShowPopup(false);
    }
    public virtual void ShowPopup(bool smooth = true)
    {
        
        //if (GameManager.instance != null && isChangeControl)
        //    GameManager.instance.pauseControl = true;
        OnPopupShown?.Invoke(this);

        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        if (smooth)
            canvasGroup.DOFade(1, .5f);
        else
            canvasGroup.alpha = 1;
        foreach (UnityEvent unityEvent in eventShow)
            unityEvent?.Invoke();

        if (isAutoHide)
        {
            if (coroutine_autoHide != null)
                StopCoroutine(coroutine_autoHide);
            coroutine_autoHide = StartCoroutine(AutoHide());
        }
        //if (soundOpen)
            //AudioManager.instance?.PlaySound(4);
        if (PopupShow != null)
        {
            PopupShow.transform.localScale = Vector3.zero;
            PopupShow.transform.DOScale(originScalePopup, .5f).SetEase(Ease.OutElastic);
        }
        if (popupSlide != null)
        {
            popupSlide.GetComponent<RectTransform>().DOAnchorPosY(fromY, 0);
            popupSlide.GetComponent<RectTransform>().DOAnchorPosY(toY, .5f);
        }
    }
    public virtual void HidePopup(bool smooth = true)
    {
        //if (GameManager.instance != null && isChangeControl)
            //GameManager.instance.pauseControl = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if (smooth)
            canvasGroup.DOFade(0, .5f);
        else
            canvasGroup.alpha = 0;
        foreach (UnityEvent unityEvent in eventHide)
            unityEvent?.Invoke();
        //if (soundClose)
            //AudioManager.instance?.PlaySound(3);
     
    }
    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(2f);
        HidePopup(true);
    }
}
