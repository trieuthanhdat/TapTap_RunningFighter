using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TD.Utilities;

public class CurvelyMoveAnimation : MonoBehaviour
{
    public RectTransform target;
    public AnimationCurve animationCurve;
    public float curveHeight = 600f;

    private RectTransform m_RectTransform;
    private Vector3 _start;
    private Vector3 _end;

    private Coroutine coroutine;

    public void Init()
    {
        m_RectTransform = GetComponent<RectTransform>();
        _start = m_RectTransform.anchoredPosition;
        _end = VectorUtils.ConvertToRectTransform(target, m_RectTransform);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (coroutine == null && Input.GetKeyDown(KeyCode.Space) == true)
        {
            Init();
            coroutine = StartCoroutine(MoveToTarget());
        }
    }
#endif
    public void StartMove()
    {
        coroutine = StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        if (!m_RectTransform) yield break;

        float duration = 0.60f;
        float time = 0f;

        while (time <= duration)
        {
            time += Time.deltaTime;

            float linearT = time / duration;
            float heightT = animationCurve.Evaluate(linearT);

            float height = Mathf.Lerp(0f, curveHeight, heightT); // you'll want the height based on screen size not just a flat 600

            m_RectTransform.anchoredPosition = Vector3.Lerp(_start, _end, linearT) + new Vector3(0f, height, 0f);

            yield return null;
        }

        coroutine = null;
    }
}
