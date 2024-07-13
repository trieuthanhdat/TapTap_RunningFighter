using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuestRewardGold
{
    void RewardGold(int quantity, System.Action<bool> callbackResult);
}
