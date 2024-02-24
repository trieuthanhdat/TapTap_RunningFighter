using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectSelectorItem : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] Transform lockMark;
    [SerializeField] Text lvText;

    private bool _isLocked;

    public bool IsLocked 
    { 
        get
        {
            return _isLocked;
        }

        set
        {
            _isLocked = value;
            lockMark.gameObject.SetActive(_isLocked);
        }
    }

    public void Init(Sprite itemData, bool isLocked, System.Action onClicked)
    {
        icon.sprite = itemData;
        IsLocked = isLocked;

        lockMark.gameObject.SetActive(isLocked);

        if (button.onClick.GetPersistentEventCount() == 0)
            button.onClick.AddListener(() => onClicked?.Invoke());
    }

    public void SetLvText(string txt)
    {
        if(lvText)
            lvText.text = txt;
    }
}
