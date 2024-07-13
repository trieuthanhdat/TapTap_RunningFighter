using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using TD.Utilities;

public class TweeningAnimationMoveToTarget : TweeningAnimation
{
    public enum AnimationMoveType
    {
        LINEAR,
        CURVE
    }
    public UnityEvent OnCompleteMovingToTarget;
    [Header("MOVE TO TARGET SETTINGS")]
    public AnimationMoveType moveType;
    public Transform targetTransform;
    public Transform objectTransform;
    public bool MoveByRect;
    [Header("MOVE TO INITIAL POSITION SETTINGS")]
    public bool MoveBack = false;
    public float moveBackDelay = 0.5f;
    public Transform initialTransform;
    [Header("MOVE CURVELY SETUP")]
    public bool autoGetControlPoint = false;
    public Transform controlPoint;
    public float curvature = 0.5f; // Control the curve intensity
    public int segments = 10;// Number of segments for the Bezier curve

    [SerializeField]
    protected Vector3 m_cachedPosition;
   
    public override void OnInit()
    {
        base.OnInit();
        if (objectTransform == null) objectTransform = this.transform;
        this.tweeningAnimationType = TweeningAnimationType.MOVE_TO_TARGET;
        SetCachedPosition();
    }

    public override void SetupFirstState()
    {
        if (MoveByRect && objectTransform.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            rect.anchoredPosition = (Vector2)m_cachedPosition;
        }
        else
        {
            objectTransform.position = m_cachedPosition;
        }
    }
    public virtual void ResetObjectPosition()
    {
        this.objectTransform.position = m_cachedPosition;
    }
    public virtual void SetTargetPosition(Transform targetTrans)
    {
        this.targetTransform = targetTrans;
    }
    public void SetCachedPosition(Vector3 newPosition)
    {
        m_cachedPosition = newPosition;
    }
    public void SetCachedPosition()
    {
        if(MoveByRect && objectTransform.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            m_cachedPosition = (Vector3)rect.anchoredPosition;
        }else
        {
            m_cachedPosition = objectTransform.position;
        }
    }

    public override Tween GetTweenAnimation()
    {
        if (objectTransform == null || targetTransform == null)
            return null;

        Sequence sequence = DOTween.Sequence();
        if (MoveByRect && objectTransform.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            switch (moveType)
            {
                case AnimationMoveType.CURVE:
                case AnimationMoveType.LINEAR:
                    Vector2 targetAnchorPos = VectorUtils.ConvertToRectTransform(targetTransform.GetComponent<RectTransform>(), rect);
                    // For RectTransform, move it to the target's screen position
                    sequence.Append(rect.DOAnchorPos(targetAnchorPos, tweenDuration)
                                        .SetEase(easeType)
                                        .OnComplete(() => { OnCompleteMovingToTarget?.Invoke(); })
                                        .Play());
                    if(MoveBack)
                    {
                        sequence.AppendInterval(moveBackDelay);
                        sequence.Append(rect.DOAnchorPos(initialTransform != null ?
                                                        initialTransform.position :
                                                        m_cachedPosition, tweenDuration)
                                            .SetEase(easeType));
                    }
                    break;
                
            }
            
        }
        else
        {
            switch (moveType)
            {
                case AnimationMoveType.LINEAR:
                    sequence.Append(objectTransform.DOMove(targetTransform.position, tweenDuration)
                                                   .SetEase(easeType)
                                                   .OnComplete(() =>
                                                   {
                                                       OnCompleteMovingToTarget?.Invoke();
                                                   }));
                    if (MoveBack)
                    {
                        sequence.AppendInterval(moveBackDelay);
                        sequence.Append(objectTransform.DOMove(initialTransform != null ?
                                                               initialTransform.position:
                                                               m_cachedPosition, tweenDuration)
                                                       .SetEase(easeType));
                    }
                    break;
                case AnimationMoveType.CURVE:
                    if (autoGetControlPoint)
                    {
                        Vector3 midpoint = VectorUtils.CalculateMidpoint(objectTransform.position, targetTransform.position);
                        Vector3 direction = (controlPoint.position - midpoint).normalized;
                        float distance = Vector3.Distance(objectTransform.position, targetTransform.position) * curvature;
                        controlPoint.position = midpoint + direction * distance;
                    }
                    Vector3[] path = VectorUtils.CalculateBezierCurvePoints(
                                     objectTransform.position,
                                     targetTransform.position,
                                     controlPoint.position, segments);
                    // For RectTransform, move it to the target's screen position
                    sequence.Append(objectTransform.DOPath(path, tweenDuration, PathType.CatmullRom)
                                                   .SetEase(easeType));
                    break;
            }
            
        }

        m_TweenAnimation = sequence;
        RegisterOnStartAndOnCompleteCallbacks();
        return m_TweenAnimation;
    }

    public TweeningAnimationMoveToTarget(
            float duration,
            Ease ease,
            Transform targetTransform,
            ref Transform objectTransform,
            bool MoveByRect,
            AnimationMoveType moveType) : base(duration, ease, TweeningAnimationType.MOVE_TO_TARGET)
    {
        this.targetTransform = targetTransform;
        this.objectTransform = objectTransform;
        this.MoveByRect = MoveByRect;
        this.moveType = moveType;
    }
}
