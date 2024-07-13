using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Amanotes
{
    public class TweeningItemLevelProgressionReward : TweeningItem
    {
        [Header("LEVEL PRROGRESSION SETTINGS")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] bool addPunchEff = true;
        [SerializeField] Vector2 scaleToPunch = new Vector2(0.1f, 0.1f);
        [SerializeField] float punchDuration = 0.4f;
        [Header("BASE settings")]
        [SerializeField] RectTransform beginRectTransform;
        [SerializeField] RectTransform targetRectTransform;
        [SerializeField] bool hideBeforeMove = false;

        public override void OnEnable()
        {
            base.OnEnable();
            OnInit();
        }

        public override void Validate()
        {
            base.Validate();
        }

        private void OnInit()
        {
            switch (tweeningType)
            {
                case TweeningType.DoMoveToTarget:
                    if (targetRectTransform)
                    {
                        if (targetRectTransform.GetComponent<CanvasGroup>())
                        {
                            targetRectTransform.GetComponent<CanvasGroup>().alpha = 1;
                        }
                        if (canvasGroup && hideBeforeMove)
                            canvasGroup.alpha = 0;
                        if (beginRectTransform)
                            rectTransform.anchoredPosition = beginRectTransform.anchoredPosition;
                    }
                    break;
                case TweeningType.DoScale:
                    if (canvasGroup)
                        canvasGroup.alpha = 0;
                    if (rectTransform)
                        rectTransform.localScale = FromScale;
                    break;
                case TweeningType.DoMoveFromBottom:
                    if (canvasGroup)
                        canvasGroup.alpha = 0;
                    if (rectTransform)
                        rectTransform.transform.localPosition = new Vector3(transform.localPosition.x, -500f, transform.localPosition.z);
                    break;
            }
        }

        public override Tween GetItemTween(GameObject item = null, Vector3 originScale = default, Vector3 originPosition = default, TweeningType newType = TweeningType.None)
        {
            //Default-DOScale
            Tween iTween = item.transform.DOScale(ToScale, FadeTime).SetEase(EaseInMode)
                    .OnComplete(() => DOScaleBackToOriginalSize(item, originScale, FadeTime));
            TweeningType tweeningType = this.tweeningType;
            if (newType != TweeningType.None)
            {
                tweeningType = newType;
            }
            switch (tweeningType)
            {
                case TweeningType.DoFlyOut:
                    iTween = item.transform.DOScale(0, FadeTime).SetEase(EaseInMode);
                    break;
                case TweeningType.DoZoomOutAndIn:
                    iTween = item.transform.DOScale(ToScale, FadeTime).SetEase(EaseInMode)
                    .OnComplete(() => DOScaleBackToOriginalSize(item, originScale, FadeTime));
                    break;
                case TweeningType.DoFadeIn:

                    break;
                case TweeningType.DoFadeOut:
                    if (item == null)
                        item = gameObject;
                    if (item.GetComponent<CanvasGroup>())
                        iTween = item.GetComponent<CanvasGroup>().DOFade(0, FadeTime).SetEase(EaseOutMode)
                                 .OnComplete(() => ProcessAfterFadingOut(item));
                    break;
                case TweeningType.DoScale:
                    iTween = item.transform.DOScale(ToScale, FadeTime).SetEase(EaseInMode)
                    .OnComplete(() => DOScaleBackToOriginalSize(item, originScale, FadeTime));
                    break;
                case TweeningType.DoMoveToTarget: //This is for level rewards
                    if (hideBeforeMove && canvasGroup)
                        canvasGroup.DOFade(1, FadeTime);
                    if(targetRectTransform && rectTransform)
                    {
                        if(targetRectTransform.GetComponent<CanvasGroup>())
                        {
                            targetRectTransform.GetComponent<CanvasGroup>().DOFade(0, FadeTime);
                        }
                        rectTransform.DOAnchorPos(targetRectTransform.anchoredPosition, FadeTime).SetEase(EaseInMode)
                        .OnComplete(delegate
                        {
                            if (addPunchEff)
                            {
                                rectTransform.DORewind();
                                rectTransform.DOPunchScale(scaleToPunch, punchDuration).SetEase(EaseOutMode);
                            }
                            rectTransform.anchoredPosition = targetRectTransform.anchoredPosition;

                            //this.PostEvent(EventID.OnLevelBadgeStamping);
                        });
                    }

                    break;
                case TweeningType.DoZoomInOutOvertime:
                    if (item == null)
                        item = gameObject;
                    Vector2 originalScale = item.transform.localScale; // Store the original scale
                    Vector2 targetScale = originalScale * ToScale; // Calculate the target scale
                                                                   // Create the zoom in tween
                    Tween zoomInTween = item.transform.DOScale(targetScale, FadeTime / 2).SetEase(EaseInMode);

                    // Create the zoom out tween
                    Tween zoomOutTween = item.transform.DOScale(originalScale, FadeTime / 2).SetEase(EaseOutMode)
                        .SetDelay(FadeTime / 2);

                    // Create a sequence of tweens
                    Sequence zoomInOutSequence = DOTween.Sequence();

                    // Add the zoom in and out tweens to the sequence
                    zoomInOutSequence.Append(zoomInTween).Append(zoomOutTween).SetLoops(-1);

                    return zoomInOutSequence;

                case TweeningType.DoMoveFromLeft:
                    item.transform.position = new Vector2(originPosition.x - 1500f, originPosition.y);
                    iTween = item.transform.DOLocalMoveX(originPosition.x, FadeTime).SetEase(EaseInMode).OnComplete(() => HasFinishedEffect = true);
                    break;
                case TweeningType.DoZoomOut:
                    iTween = item.transform.DOScale(ToScale, FadeTime).SetEase(EaseInMode).OnComplete(() => HasFinishedEffect = true);
                    break;
                default:
                    break;

            }
            return iTween;
        }
    }
}
