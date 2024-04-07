using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project_RunningFighter.Gameplay.UI
{
    /// Provides logic for a UI HUD Button to slightly shrink scale on pointer down.
    /// Also has an optional code interface for receiving notifications about down/up events (instead of just on-click)
    public class UIHUDButton : Button, IPointerDownHandler, IPointerUpHandler
    {
        // We apply a uniform 95% scale to buttons when pressed
        static readonly Vector3 k_DownScale = new Vector3(0.95f, 0.95f, 0.95f);
        public System.Action OnPointerDownEvent;
        public System.Action OnPointerUpEvent;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) { return; }
            base.OnPointerDown(eventData);
            transform.localScale = k_DownScale;
            OnPointerDownEvent?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) { return; }
            base.OnPointerUp(eventData);
            transform.localScale = Vector3.one;
            OnPointerUpEvent?.Invoke();
        }
    }
}

