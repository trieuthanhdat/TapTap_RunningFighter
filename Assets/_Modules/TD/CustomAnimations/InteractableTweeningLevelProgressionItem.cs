using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Amanotes
{
    public class InteractableTweeningLevelProgressionItem : InteractableTweeningItem, IPointerDownHandler, IPointerUpHandler
    {
        public static event EventHandler OnItemClicked;

        private bool _isSelf = true;
        private void OnEnable()
        {
            OnItemClicked += InteractableTweeningLevelProgressionItem_OnItemClicked;
        }
        private void OnDisable()
        {
            OnItemClicked -= InteractableTweeningLevelProgressionItem_OnItemClicked;
        }

        private void InteractableTweeningLevelProgressionItem_OnItemClicked(object sender, EventArgs e)
        {
            if(sender != (object)this)
            {
                _isSelf = false;

                if (tweenTarget) tweenTarget.gameObject.SetActive(false);
            }else
            {
                _isSelf = true;
                if (tweenTarget) tweenTarget.gameObject.SetActive(true);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            OnItemClicked?.Invoke(this, EventArgs.Empty);
            base.OnPointerDown(eventData);
        }
        public override void HandleTOUCH_PUNCH_SCALE_AND_SHOW_TWEENINGOBJECT()
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
            if (tweenTarget)
            {
                if (_isSelf == true && tweenTarget.gameObject.activeInHierarchy)
                {
                    StartCoroutine(tweenTarget.DOItemAnimation(tweenTarget.gameObject, true, tweenTarget.tweeningType, true));
                }
            }
        }
    }
}
