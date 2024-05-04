using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using VContainer;

public class MainMenuController : MonoBehaviour {
    private MainMenuModel _model;
    private MainMenuView _view;
    

    private void Awake() {
        _model = GetComponent<MainMenuModel>();
        _view = GetComponent<MainMenuView>();
        GetAllInfomation();
    }

    public void GetAllInfomation() {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), OnGetPlayerProfileSuccess, OnError);
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetPlayerMoneySuccess, OnError);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result) {
        try 
        {
            _model.UpdatePlayerName(result.PlayerProfile.DisplayName);
            _view.UpdatePlayerInfo(_model.PlayerName);
        } 
        catch (System.Exception e) 
        {
            Debug.Log(e);
        }
    }

    private void OnGetPlayerMoneySuccess(GetUserInventoryResult result) {
        try {
            _model.StaminaAmount = result.VirtualCurrency["ST"];
            _model.RubyAmount = result.VirtualCurrency["RB"];
            _model.GoldAmount = result.VirtualCurrency["GD"];
            _view.UpdatePlayerMoney(_model.StaminaAmount, _model.RubyAmount, _model.GoldAmount);
        } catch (System.Exception e) {
            Debug.Log(e);
        }
    }

    private void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }
}