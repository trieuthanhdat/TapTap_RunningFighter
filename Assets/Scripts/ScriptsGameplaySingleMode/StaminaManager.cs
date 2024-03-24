using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : BaseManager<StaminaManager>
{
    [SerializeField] private StaminaBar playerStaminaBar;

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    public void CreateStaminaBar(ICharacter player)
    {
        playerStaminaBar = Instantiate(playerStaminaBar);
        playerStaminaBar.transform.SetParent(transform);
        playerStaminaBar.CreateStaminaBar(player);
    }

}
