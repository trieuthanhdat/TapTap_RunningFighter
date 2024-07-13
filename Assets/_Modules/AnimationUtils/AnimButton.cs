using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
public class AnimButton : MonoBehaviour, IPointerClickHandler
{
    Vector3 originScale;
    private void Start()
    {
        originScale = transform.localScale;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(originScale * .8f, .1f).OnComplete(delegate {
            transform.DOScale(originScale, .1f);
        });
    }
}
