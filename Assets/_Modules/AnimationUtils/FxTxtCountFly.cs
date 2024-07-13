using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
public class FxTxtCountFly : MonoBehaviour
{
    public float posY = 50;
    Transform txtCount;
    private void Awake()
    {
        txtCount = transform.GetChild(0);
    }
    private void OnEnable()
    {
        txtCount.localPosition = Vector3.zero;
        txtCount.GetComponent<TextMeshPro>().color = new Color32(255, 255, 255, 255);
        txtCount.DOLocalMoveY(posY, 1f).SetEase(Ease.Linear).OnComplete(delegate {
            gameObject.SetActive(false);
        });
        txtCount.GetComponent<TextMeshPro>().DOColor(new Color32(255, 255, 255, 0), .5f).SetDelay(.5f);
    }
    public void SetTotal(int i)
    {
        txtCount.GetComponent<TextMeshPro>().text = "+" + i;
    }
}
