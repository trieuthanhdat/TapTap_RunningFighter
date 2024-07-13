using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using VContainer;

public class MainMenuController : MonoBehaviour {
    private MainMenuModel _model;
    private MainMenuView _view;

    private bool _isProfileLoaded;
    private bool _isInventoryLoaded;

    private void Awake() {
        _model = GetComponent<MainMenuModel>();
        _view = GetComponent<MainMenuView>();
        FetchAllPlayerData();
    }

    private void FetchAllPlayerData() {
        FetchPlayerProfile();
        FetchPlayerInventory();
    }

    private void FetchPlayerProfile() {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), OnPlayerProfileSuccess, OnApiError);
    }

    private void FetchPlayerInventory() {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnPlayerInventorySuccess, OnApiError);
    }

    private void OnPlayerProfileSuccess(GetPlayerProfileResult result) {
        _model.UpdatePlayerName(result.PlayerProfile.DisplayName);
        _isProfileLoaded = true;
        TryUpdateUI();
    }

    private void OnPlayerInventorySuccess(GetUserInventoryResult result) {
        // _model.StaminaAmount = result.VirtualCurrency.TryGetValue("ST", out int stamina) ? stamina : 0;
        // _model.RubyAmount = result.VirtualCurrency.TryGetValue("RB", out int ruby) ? ruby : 0;
        // _model.GoldAmount = result.VirtualCurrency.TryGetValue("GD", out int gold) ? gold : 0;

        _model.EnergyAmount = result.VirtualCurrency.TryGetValue("ER", out int energy) ? energy : 0;
        _model.StarAmount = result.VirtualCurrency.TryGetValue("ST", out int star) ? star : 0;
        _model.FairyTearAmount = result.VirtualCurrency.TryGetValue("FT", out int fairyTear) ? fairyTear : 0;
        _model.HoneyCoinAmount = result.VirtualCurrency.TryGetValue("HC", out int honeyCoin) ? honeyCoin : 0;

        _isInventoryLoaded = true;
        TryUpdateUI();
    }

    private void OnApiError(PlayFabError error) {
        Debug.LogError($"PlayFab API Error: {error.GenerateErrorReport()}");
    }

    private void TryUpdateUI() {
        if (_isProfileLoaded && _isInventoryLoaded) {
            _view.UpdatePlayerInfo(_model.PlayerName);
            _view.UpdatePlayerMoney(_model.EnergyAmount, _model.StarAmount, _model.FairyTearAmount, _model.HoneyCoinAmount);
        }
    }
}
