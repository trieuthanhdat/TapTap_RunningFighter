using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RFPlayerManager : MonoBehaviour
{
    public static RFPlayerManager instance;
    public RFPlayer player;
    public int currency;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        currency += 458;
    }
    public bool HaveEnoughMoney(int _price)
    {
        if (_price > currency)
        {
            Debug.Log("Not enough money");
            return false;
        }

        currency = currency - _price;
        return true;
    }

    public int GetCurrency() => currency;
    
}