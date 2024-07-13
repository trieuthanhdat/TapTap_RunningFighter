using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Daily Reward Configs", menuName = ("Daily Reward/Configs Data"), order = 1)]
public class DailyRewardDataConfigs : ScriptableObject
{
    public string description;
    public int maxWeeklyCycle = 3;
    public List<DailyRewardItem> listDailyReward;

#if UNITY_EDITOR
    [JsonIgnore]
    public string jsonString = "";
    [Button]
    public void GetJsonData()
    {
        jsonString = JsonConvert.SerializeObject(this);
    }
#endif
}
[System.Serializable]
public class DailyRewadItemSavedState
{
    public DailyRewardType rewardID;
    public int rewardDay;
    public DailyRewadItemSavedState(DailyRewardType id, int rewardDay)
    {
        this.rewardDay = rewardDay;
        this.rewardID = id;
    }
}

[System.Serializable]
public class DailyRewardItem
{
    public DailyRewardType rewardID;
    public int quantity;
    public int rewadDay;
    public bool canWeeklyAccumulated = true;
    public bool isBigItem;
}

public enum DailyRewardType
{
    None = 0,
    Coin = 1
}
public enum DailyRewardClaimableStatus
{
    UnClaimed,
    Claimable,
    Claimed
}