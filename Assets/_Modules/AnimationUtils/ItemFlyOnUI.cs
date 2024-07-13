using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ItemFlyOnUI : MonoBehaviour
{
    private RectTransform target;
    public AnimationCurve animationCurve;

    private RectTransform rectTransform;
    private Vector3 start;
    private Vector3 end;

    private Coroutine coroutine;

    private void Awake()
    {
       
       
    }

   
    private IEnumerator MoveToTarget()
    {
        float duration = 0.60f;
        float time = 0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float linearT = time / duration;
            float heightT = animationCurve.Evaluate(linearT);
            float widthT = animationCurve.Evaluate(linearT);

            float height = Mathf.Lerp(0f, 0f, heightT); // you'll want the height based on screen size not just a flat 600
            float width = Mathf.Lerp(0f, 0f, widthT); // you'll want the height based on screen size not just a flat 600

            rectTransform.position = Vector3.Lerp(start, end, linearT) + new Vector3(width, height, 0f);

            yield return null;
        }
        IconScale icon;
        if (target.TryGetComponent(out icon))
        {
            icon.Scale();
        }
        gameObject.SetActive(false);
        coroutine = null;
    }
    public void Fly(Transform targetFly)
    {
        rectTransform = GetComponent<RectTransform>();
        start = rectTransform.position;
        target = targetFly.GetComponent<RectTransform>();
        end = target.position;
        //transform.DOMove(targetFly.position, .5f).OnComplete(delegate
        //{
        //    IconScale icon;
        //    if (targetFly.TryGetComponent<IconScale>(out icon))
        //    {
        //        icon.Scale();
        //    }
        //    gameObject.SetActive(false);
        //});
        coroutine = StartCoroutine(MoveToTarget());
    }
}
