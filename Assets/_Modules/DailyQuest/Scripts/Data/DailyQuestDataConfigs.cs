using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyQuestDataConfigs", menuName = "Quest/DailyQuestDataConfigs")]
public class DailyQuestDataConfigs : ScriptableObject
{
    public List<QuestData> quests;
    public int questsPerDay  = 5;
    public int questCycleGap = 7;

    public bool shuffle;

    #region UNITY_EDITOR
    [JsonIgnore]
    public string json = "";
    [Button]
    public void GetJsonData()
    {
        json = JsonConvert.SerializeObject(this);
    }

    [Button]
    public void ParseJsonData()
    {
        JsonConvert.PopulateObject(json, this);
    }
    #endregion
}
[System.Serializable]
public class DailyQuestDataState
{
    public List<QuestDataState> listDataState;
    public int dayIndex;
    public DailyQuestDataState (List<QuestDataState> listDataState, int dayIndex)
    {
        this.listDataState = listDataState;
        this.dayIndex = dayIndex;
    }
}
[System.Serializable]
public class QuestDataState
{
    public string questID;
    public int currentProgress;
    public QuestState questState;

    public QuestDataState(string questID, int currentProgress, QuestState questState)
    {
        this.questID = questID;
        this.currentProgress = currentProgress;
        this.questState = questState;
    }
}

[System.Serializable]
public class QuestData
{
    public QuestActionData dailyQuestAction;
    public int dayIndex;
}
[System.Serializable]
public class QuestActionData
{
    public string questID;
    public QuestItem questItemToDo;
    public QuestActionType questActionType;
    public int requireAmount;
    public List<QuestRewardData> dailyQuestRewardDatas;
}
[System.Serializable]
public enum QuestActionType
{
    Collecting,
    Planting,
    Raising,
    Consuming,
    Creating,
    Hunting
}
[System.Serializable]
public class QuestRewardData
{
    public QuestRewardType rewardType;
    public int rewardQuantity;
}
[System.Serializable]
public enum QuestRewardType
{
    None,
    Gold
}
[System.Serializable]
public enum QuestItem
{
    Corn,
    Carrot,
    Cabbage,
    Apple,
    Chilli,
    Onion,
    Pinable,
    Potato,
    Pumpkin,
    StrawBerry,
    Cow,
    Deer,
    Chicken,
    Bird,
    Pig,
    Rabbit,
    Sheep,
    Bread,
    Coconut_Water,
    Hamburger,
    HotDog,
    IceCream,
    Meat,
    Milk,
    Popcorn,
    StrawBerry_Icream,
    ThitXienNuong
}
[System.Serializable]
public enum QuestState
{
    Todo,
    InProgress,
    ReadyToReward,
    Completed
}
