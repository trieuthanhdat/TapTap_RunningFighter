using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundClick : MonoBehaviour,IPointerClickHandler,IPointerUpHandler
{
    public int idSound;
    public void OnPointerClick(PointerEventData eventData)
    {
      
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //AudioManager.instance?.PlaySound(idSound);
    }
}
