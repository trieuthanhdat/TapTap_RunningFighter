public interface IQuestTimeProvider
{
    bool IsNewDay();
    int GetDayIndex();
    int GetWeekIndex();
}
