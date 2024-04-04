using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Project_RunningFighter.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Project_RunningFighter.Gameplay.UI
{
    public class ConnectionAnimation : BaseAnimation
    {
        [SerializeField] private float m_RotationSpeed = 360f;
        [SerializeField] private Transform m_rotateTransform;

        private void Awake()
        {
           if (m_rotateTransform == null) m_rotateTransform = GetComponent<Transform>();
        }

        protected override void PlayAnimation()
        {
            if (m_rotateTransform == null) return;

            m_rotateTransform.DORotate(new Vector3(0, 0, m_RotationSpeed), 1f, RotateMode.FastBeyond360)
                             .SetEase(Ease.Linear)
                             .SetLoops(-1, LoopType.Restart);
        }

        private void OnEnable()
        {
            PlayAnimation();
        }

        private void OnDisable()
        {
            if (m_rotateTransform == null) return;
            m_rotateTransform.DOKill(); // Stop the animation when disabled
        }
    }
}
