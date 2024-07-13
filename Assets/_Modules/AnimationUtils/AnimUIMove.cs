using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AnimUIMove : MonoBehaviour
{
    [SerializeField] Vector2 posFrom;
    [SerializeField] Vector2 posTo;
    [SerializeField] float timeMove;
    [SerializeField] Ease ease;
    RectTransform rect;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    void Start()
    {
        
    }
    private void OnEnable()
    {
        rect.anchoredPosition = posFrom;
        rect.DOAnchorPos(posTo, timeMove).SetEase(ease);
    }
}
