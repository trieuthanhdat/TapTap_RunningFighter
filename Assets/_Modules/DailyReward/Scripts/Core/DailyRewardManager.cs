using System.Collections;
using System.Collections.Generic;
using TD.GlobalTimer;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class DailyRewardManager : MonoSingleton<DailyRewardManager>
{
    [SerializeField] private float maximumInitTime = 7f;
    [SerializeField] private ITimeProvider _timeProvider; 

    public event Action<bool> OnDailyRewardManagerInited;
    public static event Action OnRewardItemClaimedSuccessfully;


    private int m_CurrentWeekIndex => _timeProvider.GetCurrentWeekIndex();
    private int m_CurrentDayIndex  => _timeProvider.GetCurrentDayIndex();

    private DataManager m_DataManager;
    //private UserData    m_UserData;

    private List<DailyRewadItemSavedState> listRewardItemClaimed = new List<DailyRewadItemSavedState>();
    public DailyRewardDataConfigs DailyRewardDataConfigs ;

    private bool _isInited = false;
    public bool IsInited => _isInited;

    public const string PLAYER_CLAIMING_REWARD_INDEX_KEY = "PLAYER_CLAIMING_REWARD_INDEX";
    public const string PLAYER_CLAIMED_REWARD_DATAS_KEY = "PLAYER_CLAIMED_REWARD_DATAS";
    public int TodayPlayerRewardClaimedIndex
    {
        get
        {
            return PlayerPrefs.GetInt(PLAYER_CLAIMING_REWARD_INDEX_KEY, 1); //DayIndex start from 1
        }
        set
        {
            PlayerPrefs.SetInt(PLAYER_CLAIMING_REWARD_INDEX_KEY, value);
        }
    }

    private void Awake()
    {
    }
    void Start()
    {
        GlobalTimerCounter.OnNewDayCome += GlobalTimerCounter_OnNewDayCome;
        Init();
    }
    private void OnDestroy()
    {
        GlobalTimerCounter.OnNewDayCome -= GlobalTimerCounter_OnNewDayCome;
    }
   
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ClearData();
            }
        }
#endif
    }
    private void Init()
    {
        _timeProvider = new GlobalTimeProvider();
        StartCoroutine(Co_Init());
    }
    private IEnumerator Co_Init()
    {
        float elapseTime = 0;
        yield return new WaitForEndOfFrame();
        //while((m_DataManager == null))
        //{
        //    elapseTime += Time.deltaTime;
        //    if (elapseTime > maximumInitTime)
        //    {
        //        OnDailyRewardManagerInited?.Invoke(false);
        //        yield break;
        //    }
        //    yield return new WaitForEndOfFrame();
        //}
        LoadData();
        _isInited = true;
        OnDailyRewardManagerInited?.Invoke(true);
    }
    private void GlobalTimerCounter_OnNewDayCome()
    {

    }

    //TEMP - PROCESS DATA LOCALLY
    private void ClearData()
    {
        PlayerPrefs.SetString(PLAYER_CLAIMED_REWARD_DATAS_KEY, string.Empty);
        PlayerPrefs.SetInt(PLAYER_CLAIMING_REWARD_INDEX_KEY, 1);
        listRewardItemClaimed.Clear();
        listRewardItemClaimed = new List<DailyRewadItemSavedState>();
        Debug.Log("DAILY REWARD MANAGER: CLEAR DATA saved");
    }
    private void SaveData()
    {
        if (listRewardItemClaimed == null) return;
        PlayerPrefs.SetString(PLAYER_CLAIMED_REWARD_DATAS_KEY, JsonConvert.SerializeObject(listRewardItemClaimed));
    }
    private void LoadData()
    {
        if (PlayerPrefs.HasKey(PLAYER_CLAIMED_REWARD_DATAS_KEY))
        {
            listRewardItemClaimed = JsonConvert.DeserializeObject<List<DailyRewadItemSavedState>>
                                    (PlayerPrefs.GetString(PLAYER_CLAIMED_REWARD_DATAS_KEY, ""));
            Debug.Log($"DRM: load data: count {listRewardItemClaimed} + {PlayerPrefs.GetString(PLAYER_CLAIMED_REWARD_DATAS_KEY, "")}");
        } else
        {
            listRewardItemClaimed = new();
        }
    }
    public DailyRewadItemSavedState GetClaimedReward(DailyRewardItem item)
    {
        if (item == null || listRewardItemClaimed == null || listRewardItemClaimed.Count == 0) 
            return null;
        foreach(DailyRewadItemSavedState savedItem in listRewardItemClaimed)
        {
            if (savedItem.rewardDay == item.rewadDay &&
                savedItem.rewardID  == item.rewardID)
                return savedItem;
        }
        return null;
    }
    public DailyRewadItemSavedState GetClaimedRewardFromDayIndex(int dayIndex)
    {
        if (listRewardItemClaimed == null || listRewardItemClaimed.Count == 0)
            return null;
        foreach (DailyRewadItemSavedState savedItem in listRewardItemClaimed)
        {
            if (savedItem.rewardDay == dayIndex)
            {
                Debug.Log($"DAILY REWARD MANAGER: get claimed reward {savedItem.rewardID} from day {dayIndex}");
                return savedItem;
            }
        }
        return null;
    }
    public DailyRewardClaimableStatus GetRewardItemClaimableStatus(DailyRewardItem item)
    {
        if (IsRewardItemClaimed(item)) return DailyRewardClaimableStatus.Claimed;

        int todayIndex = TodayPlayerRewardClaimedIndex < m_CurrentDayIndex ? TodayPlayerRewardClaimedIndex : m_CurrentDayIndex;
        return item.rewadDay <= todayIndex ? DailyRewardClaimableStatus.Claimable : DailyRewardClaimableStatus.UnClaimed;
    }
    public bool CheckIfTodayCanClaimReward()
    {
        int todayIndex   = m_CurrentDayIndex;
        int prevDayIndex = TodayPlayerRewardClaimedIndex;
        Debug.Log($"CheckIfTodayCanClaimReward => saved day index {prevDayIndex} - actual indedx {todayIndex}");
        if(todayIndex > prevDayIndex)
            return GetClaimedRewardFromDayIndex(todayIndex) == null;
        else 
        {
            return GetClaimedRewardFromDayIndex(prevDayIndex) == null;
        }
    }
    public bool IsRewardItemClaimed(DailyRewardItem item)
    {
        return GetClaimedReward(item) != null; //!= null if Already Listed as rewarded Item
    }
    public void ClaimDailyRewardItem(DailyRewardItem item, Action<bool> resultCallback = null)
    {
        if (!CheckIfTodayCanClaimReward())
        {
            resultCallback?.Invoke(false);
            Debug.LogError($"DAILY REWARD MANAGER: Already claimed today");
            return;
        }
        if (IsRewardItemClaimed(item))
        {
            resultCallback?.Invoke(false);
            return; //Item Already Claimed
        }

        switch(item.rewardID)
        {
            case DailyRewardType.Coin:
                //m_UserData.AddGold
                //(
                //    item.quantity, 
                //    (success)=>
                //    {
                //        if(success)
                //        {
                //            resultCallback?.Invoke(true);
                //            TodayPlayerRewardClaimedIndex += 1;
                //            AddToListRewardItemClaimed(item);
                //            SaveData();
                //            OnRewardItemClaimedSuccessfully?.Invoke(); //This event will make the popup refresh all items
                //            Debug.LogError($"DAILY REWARD MANAGER: successfuly reward Item {item.rewardID} + " +
                //                           $"quantity {item.quantity} + claimed at day {TodayPlayerRewardClaimedIndex}");
                //        }else
                //        {
                //            resultCallback?.Invoke(false);

                //        }

                //    }
                //);
                break;
            default:
                Debug.LogError($"DAILY REWARD MANAGER: Not yet support this reward type {item.rewardID}");
                break;
        }
    }
    private void AddToListRewardItemClaimed(DailyRewardItem item)
    {
        if (listRewardItemClaimed == null) listRewardItemClaimed = new();
        listRewardItemClaimed.Add(new DailyRewadItemSavedState(item.rewardID, item.rewadDay));
    }

}
