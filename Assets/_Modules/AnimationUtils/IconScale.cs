using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class IconScale : MonoBehaviour
{
    Vector3 originScale;
    void Start()
    {
        originScale = transform.localScale;
    }

    public void Scale()
    {
        transform.DOScale(originScale * 1.1f, 0).OnComplete(delegate
        {
            transform.DOScale(originScale, .2f);
        });
    }
}
