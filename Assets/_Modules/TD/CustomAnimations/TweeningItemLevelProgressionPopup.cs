using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Amanotes
{
    public class TweeningItemLevelProgressionPopup : MonoBehaviour
    {
        [Header("BASE SETTINGS")]
        public float delay = 0.1f;
        public float tweenTime = 0.5f;
        public float tweenTimeForCongrat = 0.3f;
        public float timeBetweenEachSequences = 0.2f;
        public float timeBetweenLevelBadgeAndCongrat = 0.1f;
        public float timeBetweenRewardItems = 0.2f;
        public Vector2 offset = new Vector2(0, 30);

        [Header("REFERENCES")]
        public RectTransform rect_Congrats;
        public RectTransform rect_YouHaveReach;
        public TweeningItemLevelProgressionReward tweenLevelBadge;
        public RectTransform rect_textReward;
        public RectTransform rect_FeatureUnlock;
        public RectTransform rect_Energy;


        private Vector2 cachePos_Congrats = Vector2.zero;
        private Vector2 cachePos_YouHaveReach = Vector2.zero;
        private Vector2 cachePos_TextReward = Vector2.zero;
        private Vector2 cachePos_FeatureUnlock = Vector2.zero;
        private Vector2 cachePos_Energy = Vector2.zero;

        private CanvasGroup cacheCanvas_Congrate = null;
        private CanvasGroup cacheCanvas_YouHaveReach = null;
        private CanvasGroup cacheCanvas_TextReward = null;
        private CanvasGroup cacheCanvas_FeatureUnlock = null;
        private CanvasGroup cacheCanvas_Energy = null;

        private void Awake()
        {
            CacheInitAnchorPos();
        }
        private void OnEnable()
        {
            OnPresetup();
            StartCoroutine(OnPlayTween());
        }
        IEnumerator OnPlayTween()
        {
            yield return new WaitForSeconds(delay);
            Sequence sequence = DOTween.Sequence();
            if (tweenLevelBadge)
            {
                sequence.Append(tweenLevelBadge.GetItemTween(tweenLevelBadge.gameObject));
            }
            sequence.AppendInterval(timeBetweenLevelBadgeAndCongrat); // Add a delay between sequences
            if (rect_Congrats)
            {
                sequence.Append(rect_Congrats.DOAnchorPos(cachePos_Congrats, tweenTimeForCongrat));
                if(cacheCanvas_Congrate != null) sequence.Join(cacheCanvas_Congrate.DOFade(1, tweenTimeForCongrat));
            }
            sequence.AppendInterval(timeBetweenEachSequences); // Add a delay between sequences
            if (rect_YouHaveReach)
            {
                sequence.Append(rect_YouHaveReach.DOAnchorPos(cachePos_YouHaveReach, tweenTime));
                if (cacheCanvas_YouHaveReach != null) sequence.Join(cacheCanvas_YouHaveReach.DOFade(1, tweenTime));
            }
            sequence.AppendInterval(timeBetweenEachSequences); // Add a delay between sequences
            if (rect_textReward)
            {
                sequence.Append(rect_textReward.DOAnchorPos(cachePos_TextReward, tweenTime));
                if (cacheCanvas_TextReward != null) sequence.Join(cacheCanvas_TextReward.DOFade(1, tweenTime));
            }
            sequence.AppendInterval(timeBetweenEachSequences); // Add a delay between sequences
            if (rect_Energy)
            {
                sequence.Append(rect_Energy.DOAnchorPos(cachePos_Energy, tweenTime));
                if (cacheCanvas_Energy != null) sequence.Join(cacheCanvas_Energy.DOFade(1, tweenTime));
            }
            sequence.AppendInterval(timeBetweenEachSequences); // Add a delay between sequences
            if (rect_FeatureUnlock)
            {
                sequence.Append(rect_FeatureUnlock.DOAnchorPos(cachePos_FeatureUnlock, tweenTime));
                if (cacheCanvas_FeatureUnlock != null) sequence.Join(cacheCanvas_FeatureUnlock.DOFade(1, tweenTime));
            }
            sequence.Play();
        }
        private void OnPresetup()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if (rect_Congrats)
            {
                rect_Congrats.anchoredPosition = new Vector2(rect_Congrats.anchoredPosition.x + offset.x, rect_Congrats.anchoredPosition.y + offset.y);
                if(rect_Congrats.GetComponent<CanvasGroup>())
                {
                    cacheCanvas_Congrate = rect_Congrats.GetComponent<CanvasGroup>();
                    cacheCanvas_Congrate.alpha = 0;
                }
            }
            if (rect_YouHaveReach)
            {
                rect_YouHaveReach.anchoredPosition = new Vector2(rect_YouHaveReach.anchoredPosition.x + offset.x, rect_YouHaveReach.anchoredPosition.y + offset.y);
                if (rect_YouHaveReach.GetComponent<CanvasGroup>())
                {
                    cacheCanvas_YouHaveReach = rect_YouHaveReach.GetComponent<CanvasGroup>();
                    cacheCanvas_YouHaveReach.alpha = 0;
                }
            }
            if (rect_textReward)
            {
                rect_textReward.anchoredPosition = new Vector2(rect_textReward.anchoredPosition.x + offset.x, rect_textReward.anchoredPosition.y + offset.y);
                if (rect_textReward.GetComponent<CanvasGroup>())
                {
                    cacheCanvas_TextReward = rect_textReward.GetComponent<CanvasGroup>();
                    cacheCanvas_TextReward.alpha = 0;
                }
            }
            if (rect_FeatureUnlock)
            {
                rect_FeatureUnlock.anchoredPosition = new Vector2(rect_FeatureUnlock.anchoredPosition.x + offset.x, rect_FeatureUnlock.anchoredPosition.y + offset.y);
                if (rect_FeatureUnlock.GetComponent<CanvasGroup>())
                {
                    cacheCanvas_FeatureUnlock = rect_FeatureUnlock.GetComponent<CanvasGroup>();
                    cacheCanvas_FeatureUnlock.alpha = 0;
                }
            }
            if (rect_Energy)
            {
                rect_Energy.anchoredPosition = new Vector2(rect_Energy.anchoredPosition.x + offset.x, rect_Energy.anchoredPosition.y + offset.y);
                if (rect_Energy.GetComponent<CanvasGroup>())
                {
                    cacheCanvas_Energy = rect_Energy.GetComponent<CanvasGroup>();
                    cacheCanvas_Energy.alpha = 0;
                }
            }
        }

        private void CacheInitAnchorPos()
        {
            if (rect_Congrats) cachePos_Congrats = rect_Congrats.anchoredPosition;
            if (rect_YouHaveReach) cachePos_YouHaveReach = rect_YouHaveReach.anchoredPosition;
            if (rect_textReward) cachePos_TextReward = rect_textReward.anchoredPosition;
            if (rect_FeatureUnlock) cachePos_FeatureUnlock = rect_FeatureUnlock.anchoredPosition;
            if (rect_Energy) cachePos_Energy = rect_Energy.anchoredPosition;
        }
    }
}
