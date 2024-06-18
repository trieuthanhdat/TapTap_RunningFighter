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

    // Energy, Star, Fairy Tear, Honey Coin
    // ER, ST, FT, HC

    // public TextMeshProUGUI StaminaText;
    // public TextMeshProUGUI RubyText;
    // public TextMeshProUGUI GoldText;
    public TextMeshProUGUI EnergyText;
    public TextMeshProUGUI StarText;
    public TextMeshProUGUI FairyTearText;
    public TextMeshProUGUI HoneyCoinText;

    void Awake()
    {
        // player name is in gameobject -> player-name-section -> player-name-holder -> player-name-txt
        PlayerNameText = GameObject.Find("player-name-section").transform.Find("player-name-holder").transform.Find("player-name-txt").GetComponent<TextMeshProUGUI>();
        // Energy, Star, Fairy Tear, Honey Coin is in gameobject -> currency-section
        // get list of children in currency-section
        Transform currencySection = GameObject.Find("currency-section").transform;
        // get list of children in currency-section
        List<Transform> currencyChildren = new List<Transform>();
        foreach (Transform child in currencySection)
        {
            currencyChildren.Add(child);
        }
        // get text component of each child
        EnergyText = currencyChildren[0].Find("holder").Find("amount").GetComponent<TextMeshProUGUI>();
        StarText = currencyChildren[1].Find("holder").Find("amount").GetComponent<TextMeshProUGUI>();
        FairyTearText = currencyChildren[2].Find("holder").Find("amount").GetComponent<TextMeshProUGUI>();
        HoneyCoinText = currencyChildren[3].Find("holder").Find("amount").GetComponent<TextMeshProUGUI>();
    }

    public void UpdatePlayerInfo(string name)
    {
        PlayerNameText.text = name;
    }

    public void UpdatePlayerMoney(int energy, int star, int fairyTear, int honeyCoin)
    {
        EnergyText.text = energy.ToString();
        StarText.text = star.ToString();
        FairyTearText.text = fairyTear.ToString();
        HoneyCoinText.text = honeyCoin.ToString();
    }
}