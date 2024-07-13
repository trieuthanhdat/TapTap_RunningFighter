using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
public class AnimScaleLoop : MonoBehaviour
{
    Vector3 originScale;
    private void Awake()
    {
        originScale = transform.localScale;
    }
    private void OnEnable()
    {
        Observable.Interval(System.TimeSpan.FromSeconds(Random.Range(3f,4f))).TakeUntilDisable(this).Subscribe(_ => {
            transform.DOScale(originScale * 1.2f, .5f).SetEase(Ease.OutBounce).OnComplete(delegate
            {
                transform.DOScale(originScale, .3f);
            });
        });
    }
   
}
