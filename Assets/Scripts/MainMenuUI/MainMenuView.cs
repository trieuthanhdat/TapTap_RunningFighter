using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI StaminaText;
    public TextMeshProUGUI RubyText;
    public TextMeshProUGUI GoldText;

    public void UpdatePlayerInfo(string name)
    {
        PlayerNameText.text = name;
    }

    public void UpdatePlayerMoney(int stamina, int ruby, int gold)
    {
        StaminaText.text = stamina.ToString();
        RubyText.text = ruby.ToString();
        GoldText.text = gold.ToString();
    }
}