using System.Collections;
using System.Collections.Generic;
using TD.UServices.CoreLobby.Infrastructure;
using UnityEngine;

namespace TD.UServices.CoreLobby.UI
{
    public class InLobbyPlayerObject : InLobbyPlayerUI
    {
        //TEMP - Will remove and replace with a Character system
        [SerializeField] protected GameObject characterObject;

        public override void SetUser(LocalPlayer localPlayer)
        {
            base.SetUser(localPlayer);
            ShowCharacterObject();
        }
        public override void ResetUI()
        {
            base.ResetUI();
            HideCharacterObject();
        }
        protected void HideCharacterObject()
        {
            if (characterObject) characterObject.SetActive(false);
        }
        protected void ShowCharacterObject()
        {
            if (characterObject) characterObject.SetActive(true);
        }
    }

}
