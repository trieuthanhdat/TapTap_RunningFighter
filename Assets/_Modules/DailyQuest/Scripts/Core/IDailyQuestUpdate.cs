public interface IDailyQuestUpdate
{
    void UpdateDailyQuests(int dayIndex);
    void MakeQuestRewards();
    void MakeQuestProgress(string questID, int progressAmount);
    bool IsCompleteAllDayQuests();
}
