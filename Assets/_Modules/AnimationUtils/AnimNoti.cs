using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class AnimNoti : MonoBehaviour
{
    Vector3 posOrigin;
    bool isSetPos;
    private void Start()
    {
        if (!isSetPos)
        {
            isSetPos = true;
            posOrigin = transform.localPosition;
            Noti();
        }
    }
    private void OnEnable()
    {
        if (isSetPos)
            Noti();
    }
    void Noti()
    {
        transform.localPosition = posOrigin;
        Observable.Interval(System.TimeSpan.FromSeconds(Random.Range(5f, 10f))).TakeUntilDisable(this).Subscribe(_ =>
        {
            transform.DOLocalMove(posOrigin + new Vector3(0, 5, 0), .3f).SetDelay(Random.Range(.3f, .7f)).OnComplete(delegate
            {
                transform.DOShakeRotation(.3f).OnComplete(delegate
                {
                    transform.localEulerAngles = Vector3.zero;
                    transform.DOLocalMove(posOrigin, .3f);
                });

            });
        });
    }
}
