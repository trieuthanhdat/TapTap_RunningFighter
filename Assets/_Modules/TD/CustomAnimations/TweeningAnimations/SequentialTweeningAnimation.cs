using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class SequentialTweeningAnimation : MonoBehaviour
{
    [SerializeField] protected GroupTweeningAnimation m_GroupAnimation = null;
    [SerializeField] protected bool m_InitOnAwake = true;

    protected Tween m_CurrentTween;
    public Tween CurrentTween => m_CurrentTween;

    public void InsertAnimationToGroup(int index, TweeningAnimation animation)
    {
        m_GroupAnimation.tweeningAnimations.Insert(index, animation);
    }

    public int GetCountGroupAnimations()
    {
        return m_GroupAnimation != null && m_GroupAnimation.tweeningAnimations != null ?
               m_GroupAnimation.tweeningAnimations.Count : 0;
    }

    public bool IsValid()
    {
        return m_GroupAnimation != null &&
               m_GroupAnimation.tweeningAnimations != null &&
               m_GroupAnimation.tweeningAnimations.Count > 0;
    }

    private void OnEnable()
    {
        if (m_InitOnAwake)
            Initialize();
    }

    public void Initialize(List<TweeningAnimation> listAnim, float waitTime, bool renew = false)
    {
        if (renew) this.m_GroupAnimation = new GroupTweeningAnimation();
        this.m_GroupAnimation.tweeningAnimations = listAnim;
        this.m_GroupAnimation.waitTimeBetweenAnimations = waitTime;
        SetupAnimationFirstState();
    }
    public void Initialize(List<TweeningAnimation> listAnim, bool renew = false)
    {
        if(renew) this.m_GroupAnimation = new GroupTweeningAnimation();
        this.m_GroupAnimation.tweeningAnimations = listAnim;
        SetupAnimationFirstState();
    }
    public void Initialize(bool deactivateOnInit = false)
    {
        SetupAnimationFirstState(deactivateOnInit);
    }

    public void ConcreteSequentialAnimation(Action OnCompleteConcretion = null)
    {
        if (!IsValid()) return;
        if(!m_GroupAnimation.useCoroutine)
        {
            HandleNonCoroutineSequentialAnimations(OnCompleteConcretion);
        }else
        {
            HandleCoroutineSequentialAnimation(OnCompleteConcretion);
        }

    }
    private void HandleCoroutineSequentialAnimation(Action OnCompleteConcretion = null)
    {
        StartCoroutine(Co_PlaySequentialAnimation(OnCompleteConcretion));
    }

    private void HandleNonCoroutineSequentialAnimations(Action callback = null)
    {
        Sequence tweenSequence = DOTween.Sequence();
        switch (m_GroupAnimation.sequentialAnimationType)
        {
            case SequentialAnimationType.Sequential:
                foreach (var item in m_GroupAnimation.tweeningAnimations)
                {
                    Tween tweenItem = item.GetTweenAnimation();
                    tweenSequence.Append(tweenItem);
                    tweenSequence.AppendInterval(m_GroupAnimation.waitTimeBetweenAnimations + item.tweenDelay);
                    tweenSequence.SetLoops(m_GroupAnimation.loopTime);
                }
                break;
            case SequentialAnimationType.Simultanious:
                foreach (var item in m_GroupAnimation.tweeningAnimations)
                {
                    Tween tweenItem = item.GetTweenAnimation();
                    tweenSequence.Join(tweenItem);
                    float delay = m_GroupAnimation.waitTimeBetweenAnimations + item.tweenDelay;
                    if (delay > 0)
                    {
                        tweenSequence.AppendInterval(delay);
                    }
                    tweenSequence.SetLoops(m_GroupAnimation.loopTime);
                }
                break;
        }
        m_CurrentTween = tweenSequence;
        callback?.Invoke();
    }

    public void PlayAnimationAtIndex(int index)
    {
        var anim = m_GroupAnimation.tweeningAnimations[index];
        if(anim)
        {
            anim.gameObject.SetActive(true);
            if (anim.TweenAnimation != null && anim.TweenAnimation.IsPlaying())
            {
                anim.TweenAnimation.Rewind();
                anim.TweenAnimation.Restart();
            }else
            {
                anim.PlayAnimation();
            }
        }
    }
    public void PlaySequenceAnimations(Action OnCompleteAction = null)
    {
        ConcreteSequentialAnimation(() =>
        {
            if (m_CurrentTween != null)
            {
                m_CurrentTween.Play().OnComplete(() => { OnCompleteAction?.Invoke(); });
            }else
            {
                OnCompleteAction?.Invoke();
            }
        });
    }
    protected IEnumerator Co_PlaySequentialAnimation(Action OnCompleteConcretion)
    {
        if (!IsValid())
        {
            yield break;
        }
        int loopCounter = 0;
        bool isInfiniteLoop = m_GroupAnimation.loopTime == -1;
        bool isOneloopComplete = false;
        while (isInfiniteLoop || loopCounter < m_GroupAnimation.loopTime)
        {
            foreach (var item in m_GroupAnimation.tweeningAnimations)
            {
                float delay = m_GroupAnimation.waitTimeBetweenAnimations + item.tweenDelay;
                item.TweenAnimation.SetLoops(m_GroupAnimation.loopTime);
                item.PlayAnimation();
                yield return new WaitForSeconds(delay);
            }
            if (!isOneloopComplete)
            {
                OnCompleteConcretion?.Invoke();
                isOneloopComplete = true;
            }
            if (!isInfiniteLoop)
            {
                loopCounter++;
            }
        }

    }
    protected void SetupAnimationFirstState(bool deactivateOnInit = false)
    {
        if (!IsValid()) return;
        foreach(var animation in m_GroupAnimation.tweeningAnimations)
        {
            if (animation == null) continue;
            animation.SetupFirstState();
            animation.gameObject.SetActive(!deactivateOnInit);
        }
    }

    public void CheckAndSetTextCurrencySubstract(int index, int substractAmount)
    {
        var anim = m_GroupAnimation.tweeningAnimations[index];
        if (anim)
        {
            anim.gameObject.SetActive(true);
            if(anim is TweeningAnimationCurrencySubstract animSubstract)
            {
                animSubstract.CheckAndSetInitialialPosition();
                animSubstract.SetCurrencySubStractAmount(substractAmount);
            }
        }
    }
}
