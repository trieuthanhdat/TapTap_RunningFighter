using TD.GlobalTimer;

public class GlobalTimeProvider : ITimeProvider
{
    public int GetCurrentDayIndex()
    {
        return GlobalTimerCounter.Instance.GetCurrentDayIndex();
    }

    public int GetCurrentWeekIndex()
    {
        return GlobalTimerCounter.Instance.GetCurrentWeekIndex();
    }

    
}

