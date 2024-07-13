using System.Collections;
using System.Collections.Generic;
using TD.GlobalTimer;
using UnityEngine;

public class DailyQuestTimeProvider : IQuestTimeProvider
{
    private GlobalTimerCounter _globalTimerCounter;

    public DailyQuestTimeProvider()
    {
        _globalTimerCounter = GlobalTimerCounter.Instance;
    }

    public bool IsNewDay()
    {
        return _globalTimerCounter.IsNewDay;
    }

    public int GetDayIndex()
    {
        return _globalTimerCounter.GetCurrentDayIndex();
    }

    public int GetWeekIndex()
    {
        return _globalTimerCounter.GetCurrentWeekIndex();
    }
}

