using DG.Tweening;
using TD.Utilities.RichTextExtension;
using TMPro;
using UnityEngine;

public class TweeningAnimationCountingNumer : TweeningAnimation
{
    public int startValue = 0;
    public int nextValue  = 0;
    public bool EnableFormat = false;
    public int MaxNumberDigits = 6; //999,999
    public string TextDelimiter = ",";

    public TextMeshProUGUI txtText;

    public override void Awake()
    {
        base.Awake();
    }
    public override void OnInit()
    {
        base.OnInit();
        if (txtText == null) txtText = gameObject.GetComponent<TextMeshProUGUI>();
        this.tweeningAnimationType = TweeningAnimationType.COUNTING_NUMBER;
    }
    
    public virtual void SetStartValue(int startVal)
    {
        startValue = startVal;
    }
    public virtual void UpdateStartValue()
    {
        startValue = nextValue;
    }
    public virtual void SetNextValue(int nextVal)
    {
        nextValue = nextVal;
    }
    public override Tween GetTweenAnimation()
    {
        if (txtText == null)
            return null;

        m_TweenAnimation = DOTween.To(() => startValue,
                                   x => txtText.text = RichTextFormatHelper.RichTextFormat(x, MaxNumberDigits, TextDelimiter), nextValue, tweenDuration)
                                  .SetEase(easeType)
                                  .OnStart(() => OnStartAnimationEvent?.Invoke())
                                  .OnComplete(() => OnCompleteAnimationEvent?.Invoke());
        return m_TweenAnimation;
    }
    public TweeningAnimationCountingNumer(float duration, Ease ease, int nextValue, int startValue, TextMeshProUGUI txtText) : base(duration, ease, TweeningAnimationType.COUNTING_NUMBER)
    {
        this.nextValue  = nextValue;
        this.txtText    = txtText;
        this.startValue = startValue;
    }
}