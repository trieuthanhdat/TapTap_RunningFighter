using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuModel : MonoBehaviour
{
    // model stores data
    private string _playerName;
    private int _staminaAmount;
    private int _rubyAmount;
    private int _goldAmount;

    public string PlayerName { get => _playerName; set => _playerName = value; }
    public int StaminaAmount { get => _staminaAmount; set => _staminaAmount = value; }
    public int RubyAmount { get => _rubyAmount; set => _rubyAmount = value; }
    public int GoldAmount { get => _goldAmount; set => _goldAmount = value; }

    public void UpdatePlayerName(string name)
    {
        _playerName = name;
    }

    public void UpdatePlayerMoney(int stamina, int ruby, int gold)
    {
        _staminaAmount = stamina;
        _rubyAmount = ruby;
        _goldAmount = gold;
    }
}
