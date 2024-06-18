using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuModel : MonoBehaviour
{
    // model stores data
    private string _playerName;
    private int _energyAmount;
    private int _starAmount;
    private int _fairyTearAmount;
    private int _honeyCoinAmount;

    public string PlayerName { get => _playerName; set => _playerName = value; }
    public int EnergyAmount { get => _energyAmount; set => _energyAmount = value; }
    public int StarAmount { get => _starAmount; set => _starAmount = value; }
    public int FairyTearAmount { get => _fairyTearAmount; set => _fairyTearAmount = value; }
    public int HoneyCoinAmount { get => _honeyCoinAmount; set => _honeyCoinAmount = value; }

    public void UpdatePlayerName(string name)
    {
        _playerName = name;
    }

    public void UpdatePlayerMoney(int energy, int star, int fairyTear, int honeyCoin)
    {
        _energyAmount = energy;
        _starAmount = star;
        _fairyTearAmount = fairyTear;
        _honeyCoinAmount = honeyCoin;
    }
}
