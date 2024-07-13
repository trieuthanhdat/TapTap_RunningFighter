using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweeningAnimationFillAmount : TweeningAnimation
{
    public enum AnimationFillAmountMethod
    {
        IMAGE,
        SLIDER
    }
    public AnimationFillAmountMethod fillMethod;
    public GameObject objectFillAmount;
    public float startValue;
    public float NextValue;

    protected Image m_Image;
    protected Slider m_Slider;


    public override void OnInit()
    {
        base.OnInit();
        if (objectFillAmount == null) objectFillAmount = this.gameObject;
        this.tweeningAnimationType = TweeningAnimationType.FILL_AMOUNT;
    }
    public virtual void SetNextValue(float nextVal)
    {
        NextValue = nextVal;
    }
    public virtual void SetStartValue(float startVal)
    {
        startValue = startVal;
    }
    
    public override void SetupFirstState()
    {
        base.SetupFirstState();
        switch(fillMethod)
        {
            case AnimationFillAmountMethod.IMAGE:
                if(objectFillAmount.TryGetComponent<Image>(out Image img))
                {
                    m_Image = img;
                    m_Image.fillAmount = 0;
                }
                break;
            case AnimationFillAmountMethod.SLIDER:
                if(objectFillAmount.TryGetComponent<Slider>(out Slider slider))
                {
                    m_Slider = slider;
                    m_Slider.value = 0;
                }
                break;
        }
    }
    public virtual void UpdateProgressValue(float progress)
    {
        switch (fillMethod)
        {
            case AnimationFillAmountMethod.IMAGE:
                if (m_Image) m_Image.fillAmount = progress;
                break;
            case AnimationFillAmountMethod.SLIDER:
                if (m_Slider) m_Slider.value = progress;
                break;
        }
    }
    public override Tween GetTweenAnimation()
    {
        if (objectFillAmount == null) return null;
        switch(fillMethod)
        {
            case AnimationFillAmountMethod.IMAGE:
                if (m_Image)
                {
                    m_TweenAnimation = m_Image.DOFillAmount(NextValue, tweenDuration)
                                              .SetEase(easeType);
                }
                break;
            case AnimationFillAmountMethod.SLIDER:
                if (m_Slider)
                {
                    m_TweenAnimation = m_Slider.DOValue(NextValue, tweenDuration)
                                               .SetEase(easeType);
                }
                break;
        }
        RegisterOnStartAndOnCompleteCallbacks();
        return m_TweenAnimation;
    }

    public TweeningAnimationFillAmount(float duration, Ease ease, float nextValue, ref GameObject objectFillAmount) : base(duration, ease, TweeningAnimationType.FILL_AMOUNT)
    {
        this.NextValue = nextValue;
        this.objectFillAmount = objectFillAmount;
    }
}
