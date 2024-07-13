using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IpadContentSettingsController : MonoBehaviour
{
    public List<IpadContentSettings> listSettings = new List<IpadContentSettings>();

    public void AddToList(IpadContentSettings item)
    {
        listSettings.Add(item);
    }

    public void MakeChanges()
    {
        if(listSettings != null && listSettings.Count > 0)
        {
            foreach(IpadContentSettings item in listSettings)
            {
                item.ApplySettingsInstantly();
            }
        }
    }


}
