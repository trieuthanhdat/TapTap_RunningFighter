using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoSingleton<DailyQuestManager>, IDailyQuestSaveAndLoad, IDailyQuestUpdate
{
    [SerializeField] private IQuestRewardGold GoldRewarder;
    [SerializeField] private DailyQuestDataConfigs m_QuestDataConfigs;
    [SerializeField] private DailyQuestDataState   m_QuestDataState;
    [SerializeField] private List<QuestData> m_ListTodayQuests = new List<QuestData>();
    private IQuestTimeProvider m_TimeProvider;
    private const string QUEST_STATES_KEY = "DailyQuestDataState";

    public DailyQuestDataConfigs QuestDataConfigs => m_QuestDataConfigs;

    private void Start()
    {
        m_TimeProvider = new DailyQuestTimeProvider();
        LoadDailyQuests();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            int randomInd = Random.Range(0, m_ListTodayQuests.Count - 1);
            MakeQuestProgress(m_ListTodayQuests[randomInd].dailyQuestAction.questItemToDo, m_ListTodayQuests[randomInd].dailyQuestAction.questActionType, 1);
            //MakeQuestProgress(QuestItem.Cow, QuestActionType.Hunting, 1); EXAMPLE
        }
    }

    public void LoadDailyQuests()
    {
        string jsonData = PlayerPrefs.GetString(QUEST_STATES_KEY, string.Empty);

        if (m_TimeProvider.IsNewDay() || string.IsNullOrEmpty(jsonData)) //Refresh new quests when new day comes
        {
            m_QuestDataState = new DailyQuestDataState(new List<QuestDataState>(), m_TimeProvider.GetDayIndex());
            UpdateDailyQuests(m_TimeProvider.GetDayIndex());
            Debug.Log($"DAILY QUEST MANAGER: new day => quest datas {jsonData}");
        }
        else
        {
            m_QuestDataState = JsonUtility.FromJson<DailyQuestDataState>(jsonData);
            GetAllQuestDataFromQuestState();
            Debug.Log($"DAILY QUEST MANAGER: current quest datas {jsonData}");
        }
        
    }
    private void GetAllQuestDataFromQuestState()
    {
        foreach(var state in m_QuestDataState.listDataState)
        {
            QuestData quest = m_QuestDataConfigs.quests.Find(q => q.dailyQuestAction.questID == state.questID);
            if(quest != null)
            {
                m_ListTodayQuests.Add(quest);
            }
        }
    }
    public void SaveDailyQuests()
    {
        string jsonData = JsonUtility.ToJson(m_QuestDataState);
        PlayerPrefs.SetString(QUEST_STATES_KEY, jsonData);
        Debug.Log($"DAILY QUEST MANAGER: save data {jsonData}");
    }

    public void UpdateDailyQuests(int dayIndex)
    {
        m_QuestDataState.listDataState.Clear();
        m_ListTodayQuests = GetDailyQuests(dayIndex);
        foreach (var quest in m_ListTodayQuests)
        {
            m_QuestDataState.listDataState.Add(new QuestDataState(quest.dailyQuestAction.questID, 0, QuestState.Todo));
        }
        m_QuestDataState.dayIndex = dayIndex;
        SaveDailyQuests();
    }

    private List<QuestData> GetDailyQuests(int dayIndex)
    {
        List<QuestData> dailyQuests = new List<QuestData>();
        List<QuestData> availableQuests = new List<QuestData>(m_QuestDataConfigs.quests);

        if (m_QuestDataConfigs.shuffle)
        {
            availableQuests.Shuffle();
        }

        int totalQuests   = availableQuests.Count;
        int questsPerDay  = m_QuestDataConfigs.questsPerDay;
        int questCycleGap = m_QuestDataConfigs.questCycleGap;
        int startIndex    = (dayIndex % questCycleGap) * questsPerDay;

        if (startIndex >= totalQuests)
        {
            Debug.LogWarning("Start index is out of range. Resetting to zero.");
            startIndex = 0;
        }

        for (int i = 0; i < questsPerDay; i++)
        {
            int index = (startIndex + i) % totalQuests;
            var quest = availableQuests[index];
            if (quest != null)
            {
                dailyQuests.Add(quest);
            }
        }

        return dailyQuests;
    }

    public void MakeQuestRewards()
    {
        foreach (var questState in m_QuestDataState.listDataState)
        {
            if (questState.questState == QuestState.ReadyToReward)
            {
                RewardPlayer(questState.questID);
                questState.questState = QuestState.Completed;
            }
        }
        SaveDailyQuests();
    }
    public void MakeQuestProgress(QuestItem questItem, QuestActionType actionType,  int progressAmount)
    {
        List<QuestData> listQuest = GetQuestDatasByItemAndActionType(questItem, actionType);
        if (listQuest == null || listQuest.Count == 0)
        {
            Debug.Log(" DAILY QUEST MANAGER: cannot get any quest => return");
            return;
        }
        Debug.Log($" DAILY QUEST MANAGER: Found {listQuest.Count} quests with questItem {questItem} - actionType {actionType}, progress {progressAmount}");
        foreach (var quest in listQuest)
        {
            if (quest == null || quest.dailyQuestAction == null) continue;

            string questID = quest.dailyQuestAction.questID;
            var questState = m_QuestDataState.listDataState.Find(q => q.questID == questID);
            if (questState != null)
            {
                questState.currentProgress += progressAmount;
                UpdateQuestState(questState);
                SaveDailyQuests();
            }
        }
        
    }
    public void MakeQuestProgress(string questID, int progressAmount)
    {
        var questState = m_QuestDataState.listDataState.Find(q => q.questID == questID);
        if (questState != null)
        {
            questState.currentProgress += progressAmount;
            UpdateQuestState(questState);
            SaveDailyQuests();
        }
    }

    private List<QuestData> GetQuestDatasByItemAndActionType(QuestItem questItem, QuestActionType actionType)
    {
        return m_QuestDataConfigs.quests.FindAll(q =>
                q.dailyQuestAction.questActionType == actionType &&
                q.dailyQuestAction.questItemToDo   == questItem);
    }
    public bool IsCompleteAllDayQuests()
    {
        foreach (var questState in m_QuestDataState.listDataState)
        {
            if (questState.questState != QuestState.Completed)
            {
                return false;
            }
        }
        return true;
    }

    public void RewardPlayer(string questID)
    {
        QuestActionData questAction = GetQuestById(questID);

        foreach(var reward in questAction.dailyQuestRewardDatas)
        {
            switch(reward.rewardType)
            {
                default:
                case QuestRewardType.Gold:
                    //Reward Gold here
                    GoldRewarder.RewardGold(reward.rewardQuantity, (success)=>
                    {
                        if(success)
                        {

                        }else
                        {
                            return;
                        }
                    });
                    break;
            }
        }
    }

    private QuestActionData GetQuestById(string questID)
    {
        foreach (var quest in m_QuestDataConfigs.quests)
        {
            if (quest == null) continue;
            if (quest.dailyQuestAction == null) continue;

            if (quest.dailyQuestAction.questID == questID)
            {
                return quest.dailyQuestAction;
            }
        }
        return null;
    }

    public void UpdateQuestState(QuestDataState questDataState)
    {
        if (questDataState.currentProgress == 0)
        {
            questDataState.questState = QuestState.Todo;
        }
        else if (questDataState.currentProgress > 0 && 
                 questDataState.currentProgress < GetQuestById(questDataState.questID).requireAmount)
        {
            questDataState.questState = QuestState.InProgress;
        }
        else if (questDataState.currentProgress >= GetQuestById(questDataState.questID).requireAmount)
        {
            questDataState.questState = QuestState.ReadyToReward;
        }
    }
    
}