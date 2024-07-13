using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace TD.GlobalTimer
{
    public enum DateTimeFormatType
    {
        None,
        Default,//Include Year, Month, Day, Hour, Minute, second
        Hours,
        Hours_Minutes,
        Hour_Minutes_Seconds,
        Days,
        Days_Hours,
        Days_Hours_Minutes,
        Days_Hours_Minutes_Seconds,
        Minutes,
        Minutes_Seconds,
        Seconds
    }
    public class GlobalTimerCounter : MonoSingleton<GlobalTimerCounter>
    {
        [SerializeField] DateTimeFormatType dateTimeFormat = DateTimeFormatType.Hour_Minutes_Seconds;
        [ReadOnly][SerializeField] float secondThreshold = 1; //1 second

        [Tooltip("Press (LControl + T) to Reset the timer methods, This flag marks whether it should Get Server's DateTime agian or not")]
        [SerializeField] bool GetServerTimeAgainWhenReset = false;
        public static event Action OnNewDayCome;
        public static void OnNewDayComeTriggered()
        {
            OnNewDayCome?.Invoke();
            Debug.Log("<=====ON NEW DAY COME!!!!=====>");
        }

        public static event Action OnNewWeekCome;
        public static void OnNewWeekComeTriggered()
        {
            OnNewWeekCome?.Invoke();
            Debug.Log("<=====ON NEW WEEK COME!!!!=====>");
        }

        #region ___PROPERTIES___

        [SerializeField][ReadOnly] 
        private int m_CurrentDayIndex = 0;
        [SerializeField][ReadOnly]
        private int m_CurrentWeekIndex = 0;
        [SerializeField][ReadOnly]
        private TimeSpan m_TimeLeft;

        private DateTime m_CurrentDate;
        private DateTime m_NextTimeReset;
        private DateTime m_AntiCheatedDateTrack;
        
        private float m_TimeThreholdTrack = 0;
        private float m_TimeSinceStartup = 0;

        private bool m_CanTimer = false;
        public bool CanTimer
        {
            get => m_CanTimer;
            set => m_CanTimer = value;
        }

        private bool _HasGotServerTime;
        public bool HasGotServerTime
        {
            get => _HasGotServerTime;
            set => _HasGotServerTime = value;
        }

        private bool _isInited = false;
        public bool IsInited
        {
            get => _isInited;
            set => _isInited = value;
        }

        private bool _isNewDay = false;
        public bool IsNewDay
        {
            get => _isNewDay;
            set
            {
                if (value == true) OnNewDayCome?.Invoke();
                _isNewDay = value;
            }
        }


        private bool _isNewWeek = false;
        public bool IsNewWeek
        {
            get => _isNewWeek;
            set
            {
                if (value == true) OnNewWeekCome?.Invoke();
                _isNewWeek = value;
            }
        }
        #endregion

        public const string DATE_TIME_FORMAT = "{0:00}h-{1:00}m-{2:00}s";
        public const string SYSTEM_TIMER_STRING_KEY = "NewSysDateTime";
        public const string SYSTEM_DAY_INDEX_STRING_KEY = "dayTimeIndex";
        public const string SYSTEM_WEEK_INDEX_STRING_KEY = "weekTimeIndex";
        public const string SERVER_TIME_URL = "https://worldtimeapi.org/api/ip";

        public struct GlobalTimeStruct
        {
            public string datetime;
        }

        //=====>UNITY METHODS<=====//
        #region ___UNITY METHODS___
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
            m_TimeSinceStartup = Time.realtimeSinceStartup;
            StartCoroutine(Co_GetServerTimeWebRequest());
        }
        private void OnEnable()
        {
            OnNewDayCome += GlobalTimer_OnNewDayCome;
            OnNewWeekCome += GlobalTimer_OnNewWeekCome; // Subscribe to the new week event
        }

        private void OnDisable()
        {
            OnNewDayCome -= GlobalTimer_OnNewDayCome;
            OnNewWeekCome -= GlobalTimer_OnNewWeekCome; // Unsubscribe from the new week event
        }

        private void Update()
        {
#if UNITY_EDITOR
        
        if(Input.GetKey(KeyCode.LeftControl))
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                ResetTimer();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                OnNewDayComeTriggered();
            }
        }
#endif
            UpdateTimer();
        }

        //This help preventing the timer to stop when the User Unforcus,
        //When he forcus agian the timer will show wrong
        private float _UnfocusTimestamp = 0;
        private float _FocusTimestamp = 0;
        private bool _WasFocused = true;

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (!_WasFocused)
                {
                    _FocusTimestamp = Time.realtimeSinceStartup;
                    // Calculate the time it was unfocused
                    float unfocusedDuration = _FocusTimestamp - _UnfocusTimestamp;
                    m_AntiCheatedDateTrack = m_AntiCheatedDateTrack.AddSeconds(unfocusedDuration);
                    Debug.Log("GLOBAL TIMER: Paused => time since startup " + m_TimeSinceStartup +
                              " UnfocusTimestamp " + _UnfocusTimestamp +
                              " FocusTimestamp " + _FocusTimestamp + " " +
                              " unfocusedDuration " + unfocusedDuration);
                }
            }
            else
            {
                _UnfocusTimestamp = Time.realtimeSinceStartup;
                Debug.Log("GLOBAL TIMER: Unpaused => time since startup " + _UnfocusTimestamp);
            }
            _WasFocused = !pauseStatus; // invert pauseStatus to match wasFocused
        }

        #endregion
        //======SERVER TIMER METHODS======//
        #region ___SERVER TIMER METHODS___
        private bool m_IsReseting = false;
        private void ResetTimer(bool hardReset = true)
        {
            CanTimer  = false;
            IsInited  = false;
            IsNewDay  = false;
            IsNewWeek = false;
            HasGotServerTime = false;
            if(hardReset)
            {
                PlayerPrefs.DeleteKey(SYSTEM_DAY_INDEX_STRING_KEY);
                PlayerPrefs.DeleteKey(SYSTEM_WEEK_INDEX_STRING_KEY);
                PlayerPrefs.DeleteKey(SYSTEM_TIMER_STRING_KEY);
            }
            IEnumerator resetRoutine = null;
            if (GetServerTimeAgainWhenReset || hardReset)
            {
                resetRoutine = Co_GetServerTimeWebRequest();
                StartCoroutine(resetRoutine);
            }
            if(resetRoutine != null)
            {
                float elapseTime = 0;
                while(resetRoutine.MoveNext())
                {
                    elapseTime += Time.deltaTime;
                    if (elapseTime >= 10f)
                        break;

                    m_IsReseting = true;
                }
                m_IsReseting = false;
            }else
            {
                m_IsReseting = false;
            }
            
        }
        private IEnumerator Co_GetServerTimeWebRequest()
        {
            using (var webRequest = UnityWebRequest.Get(SERVER_TIME_URL))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    GlobalTimeStruct globalTime = JsonConvert.DeserializeObject<GlobalTimeStruct>(webRequest.downloadHandler.text);

                    // Parse the datetime string as DateTimeOffset
                    DateTimeOffset serverDateTimeOffset;
                    if (DateTimeOffset.TryParse(globalTime.datetime, null, DateTimeStyles.RoundtripKind, out serverDateTimeOffset))
                    {
                        // Convert DateTimeOffset to DateTime
                        DateTime serverTime = serverDateTimeOffset.DateTime;
                        DateTime nextDay = TryGetSaveDate(out DateTime saveDay) ?
                                              saveDay.AddDays(1) :
                                              serverTime.AddDays(1);
                        //Ensure next day start at midnight (00:00:00) 
                        nextDay = nextDay.Date;
                        UpdateNextTimeReset(nextDay);
                        ParseDateTime(serverTime);
                        _HasGotServerTime = true;
                        _isInited = true;
                        Debug.Log($"GLOBAL TIMER: TIME OFFSET SECONDS: Tommorow: {nextDay} - Today: {serverTime}");
                        Refresh();

                    }
                    //else
                    //{
                    //    Debug.LogError("Failed to parse server time: " + webRequest.downloadHandler.text);
                    //    //Try another method
                    //    StartCoroutine(Co_GetServerTimeUWR(SERVER_TIME_URL));
                    //}
                }
                else
                {
                    Debug.LogError("Failed to retrieve server time: " + webRequest.error);
                    //Fall back to use local timer
                    StartCoroutine(Co_GetServerTimeUWR(SERVER_TIME_URL));
                }
            }
        }

        private int _retryTimeAllowed = 5;
        private int _retryTime = 0;
        /// <summary>
        /// This Code from anh HIEN -
        /// get Server Time Unity Web Service Request
        /// Used as a backup method When Co_GetServerTimeWebRequest() failes
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private IEnumerator Co_GetServerTimeUWR(string uri)
        {
            while (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitForEndOfFrame();
            Debug.Log("GLOBAL TIMER: use other method to get server time, uri => " + uri);
            using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            DateTime onlineDate = DateTime.MinValue;

            bool success = false;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:

                    _retryTime++;
                    if (_retryTime < _retryTimeAllowed)
                    {
                        StopCoroutine(Co_GetServerTimeUWR(SERVER_TIME_URL));
                        StartCoroutine(Co_GetServerTimeUWR(SERVER_TIME_URL));
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    success = true;
                    try
                    {
                        OnlineDateTimeData dateTimeData = JsonUtility.FromJson<OnlineDateTimeData>(webRequest.downloadHandler.text);
                        if (dateTimeData.IsValid())
                        {
                            if (DateTime.TryParse(dateTimeData.datetime, out onlineDate))
                            {
                                Debug.Log("GLOBAL TIMER: Server tim = " + onlineDate.ToString("yyyymmddhhmmss"));
                            }
                            else
                            {
                                Debug.Log("GLOBAL TIMER: Server time - cannot parse DATE");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("GLOBAL TIMER: exception:" + ex.ToString());
                    }

                    break;
            }
            _HasGotServerTime = success;
            DateTime nextDay = TryGetSaveDate(out DateTime saveDay) ?
                                              saveDay.AddDays(1) :
                                              onlineDate.AddDays(1);
            //Ensure next day start at midnight (00:00:00) 
            nextDay = nextDay.Date;
            UpdateNextTimeReset(nextDay);
            if (success)
            {
                ParseDateTime(onlineDate != DateTime.MinValue ? onlineDate : DateTime.Now);
            }
            else
            {
                Debug.Log("GLOBAL TIMER: Failed to get server time => use device time");
                ParseDateTime(DateTime.Now);
            }
            Refresh();
            _isInited = true;
        }
        private void ParseDateTime(DateTime now)
        {
            m_CurrentDate = now;
            //Start AntiCheat here
            m_AntiCheatedDateTrack = m_CurrentDate;
        }
        #endregion
        //======CLASS METHODS======//
        #region ___CLASS METHODS___
        private void GlobalTimer_OnNewDayCome()
        {
            m_CurrentDayIndex = PlayerPrefs.GetInt(SYSTEM_DAY_INDEX_STRING_KEY);
            m_CurrentDayIndex += 1;

            Debug.Log("NEW DAY COME!!!!!");
            StoreDateAndIndex();
            IsNewDay = false;
        }
        private void GlobalTimer_OnNewWeekCome()
        {
            m_CurrentWeekIndex = PlayerPrefs.GetInt(SYSTEM_WEEK_INDEX_STRING_KEY);
            m_CurrentWeekIndex += 1;

            StoreDateAndIndex();
            IsNewWeek = false;
        }
        private void Refresh()
        {
            LoadCurrentDayIndex();
            CalculateDayIndex();
            CalculateWeekIndex();
        }

        private void StoreDateAndIndex()
        {
            if (!IsInited) return;

            PlayerPrefs.SetInt(SYSTEM_DAY_INDEX_STRING_KEY, m_CurrentDayIndex);
            PlayerPrefs.SetInt(SYSTEM_WEEK_INDEX_STRING_KEY, m_CurrentWeekIndex); // Store the week index
            PlayerPrefs.SetString(SYSTEM_TIMER_STRING_KEY, m_CurrentDate.ToBinary().ToString());
            Debug.Log($"GLOBAL TIMER: store date: {m_CurrentDate} - day index: {m_CurrentDayIndex} - week index: {m_CurrentWeekIndex}");
        }

        private void LoadCurrentDayIndex()
        {
            int savedDayIndex = PlayerPrefs.GetInt(SYSTEM_DAY_INDEX_STRING_KEY, -1);
            m_CurrentDayIndex = savedDayIndex == -1 ? 0 : savedDayIndex;

            int savedWeekIndex = PlayerPrefs.GetInt(SYSTEM_WEEK_INDEX_STRING_KEY, -1);
            m_CurrentWeekIndex = savedWeekIndex == -1 ? 0 : savedWeekIndex; // Load the week index

            Debug.Log("GLOBAL TIMER: load current day index " + m_CurrentDayIndex + " - week index: " + m_CurrentWeekIndex);

        }
        private bool TryGetSaveDate(out DateTime dateTime)
        {
            long savedDateTime;
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(SYSTEM_TIMER_STRING_KEY, "")))
            {
                dateTime = m_CurrentDate;
                return false;
            }

            savedDateTime = Convert.ToInt64(PlayerPrefs.GetString(SYSTEM_TIMER_STRING_KEY, ""));
            dateTime = DateTime.FromBinary(savedDateTime);
            return true;
        }
        private void UpdateNextTimeReset(DateTime nextDate)
        {
            m_NextTimeReset = nextDate;
        }
        private void UpdateTimer()
        {
            if (m_IsReseting) return;
            if (!CanTimer) return;
            if (!IsInited) return;

            m_TimeThreholdTrack += Time.unscaledDeltaTime;
            if (m_TimeThreholdTrack >= secondThreshold)
            {
                m_AntiCheatedDateTrack = m_AntiCheatedDateTrack.AddSeconds(1);
                m_TimeLeft = m_NextTimeReset - m_AntiCheatedDateTrack;
                m_TimeThreholdTrack = 0f;
            }

            // If the time difference is negative, add one day to the next reset time
            if (m_TimeLeft.TotalSeconds < 0)
            {
                m_TimeLeft = new TimeSpan();
                IsNewDay = true;
                m_IsReseting = true;
                //GET SERVER TIME AGIAN
                ResetTimer(false);
            }
        }
        private void CalculateWeekIndex()
        {
            if (_HasGotServerTime)
            {
                if (TryGetSaveDate(out DateTime oldDate))
                {
                    TimeSpan difference = m_CurrentDate.Subtract(oldDate);
                    int weeksPassed = difference.Days / 7; // Calculate the number of weeks passed
                    m_CurrentWeekIndex += weeksPassed;

                    if (weeksPassed > 0)
                    {
                        IsNewWeek = true;
                    }
                    else
                    {
                        IsNewWeek = false;
                    }
                }
                else
                {
                    if (m_CurrentWeekIndex == 0)
                    {
                        IsNewWeek = true;
                        StoreDateAndIndex();
                        Debug.Log("GLOBAL TIMER: It's first week!!");
                    }
                }
            }
        }

        private void CalculateDayIndex()
        {
            // Store the current time when it starts
            if (_HasGotServerTime)
            {
                if (TryGetSaveDate(out DateTime oldDate))
                {
                    TimeSpan difference = m_CurrentDate.Subtract(oldDate);
                    m_CanTimer = true;
                    Debug.Log("GLOBAL TIMER: CalculateDayIndex: diff " + difference);
                    if (difference.Days > 0)
                    {
                        m_CurrentDayIndex += 1;
                        IsNewDay = true;
                    }
                    else if (difference.Days <= 0)
                    {
                        IsNewDay = false;
                    }
                }
                else
                {
                    if (m_CurrentDayIndex == 0)
                    {
                        m_CanTimer = true;
                        IsNewDay = true;

                        StoreDateAndIndex();
                        Debug.Log("GLOBAL TIMER: It's first day!!");
                        return;
                    }
                }
            }

        }
        #endregion
        //======UTILITIES======//
        #region ___UTILITIES METHOD___
        //FOR TESTING ONLY
        [Obsolete]
        public void SetNextTimeReset_Till_One_Hour_Left()
        {
            // Set m_AntiCheatedDateTrack to one hour less than m_NextTimeReset
            m_AntiCheatedDateTrack = m_NextTimeReset.AddHours(-1);
        }
        [Obsolete]
        public void SetNextTimeReset_Till_One_Min_Left()
        {
            // Set m_AntiCheatedDateTrack to one minute less than m_NextTimeReset
            m_AntiCheatedDateTrack = m_NextTimeReset.AddMinutes(-1);
        }
        public int GetCurrentWeekIndex()
        {
            return m_CurrentWeekIndex;
        }
        public int GetCurrentDayIndex()
        {
            return m_CurrentDayIndex;
        }
        public string GetDateTimeFormatString(DateTimeFormatType dateTimeFormatType)
        {
            string format = "";
            switch (dateTimeFormatType)
            {
                case DateTimeFormatType.Hours:
                    format = "{0:00}h";
                    break;
                case DateTimeFormatType.Hours_Minutes:
                    format = "{0:00}h:{1:00}m";
                    break;
                case DateTimeFormatType.Hour_Minutes_Seconds:
                    format = "{0:00}h:{1:00}m:{2:00}s";
                    break;
                case DateTimeFormatType.Days:
                    format = "{0}d";
                    break;
                case DateTimeFormatType.Days_Hours:
                    format = "{0}d:{1:00}h";
                    break;
                case DateTimeFormatType.Days_Hours_Minutes:
                    format = "{0}d:{1:00}h:{2:00}m";
                    break;
                case DateTimeFormatType.Days_Hours_Minutes_Seconds:
                    format = "{0}d:{1:00}h:{2:00}m:{3:00}s";
                    break;
                case DateTimeFormatType.Minutes:
                    format = "{0:00}m";
                    break;
                case DateTimeFormatType.Minutes_Seconds:
                    format = "{0:00}m:{1:00}s";
                    break;
                case DateTimeFormatType.Seconds:
                    format = "{0:00}s";
                    break;
                case DateTimeFormatType.Default:
                case DateTimeFormatType.None:
                default:
                    format = DATE_TIME_FORMAT; // Default format
                    break;
            }
            return format;
        }
        public string StartFormating(DateTimeFormatType dateTimeFormatType, TimeSpan timeDifference)
        {
            string format = GetDateTimeFormatString(dateTimeFormatType);
            int totalHours       = (int)timeDifference.TotalHours;
            int totalDays        = (int)timeDifference.TotalDays;
            int totalMinutes     = (int)timeDifference.TotalMinutes;
            int totalSeconds     = (int)timeDifference.TotalSeconds;
            int remainingHours   = timeDifference.Hours;
            int remainingMinutes = timeDifference.Minutes;
            int remainingSeconds = timeDifference.Seconds;
            
            string formattedTime = "";

            switch (dateTimeFormatType)
            {
                case DateTimeFormatType.Hours:
                    formattedTime = string.Format(format, totalHours);
                    break;
                case DateTimeFormatType.Hours_Minutes:
                    formattedTime = string.Format(format, totalHours, remainingMinutes);
                    break;
                case DateTimeFormatType.Hour_Minutes_Seconds:
                    formattedTime = string.Format(format, totalHours, remainingMinutes, remainingSeconds);
                    break;
                case DateTimeFormatType.Days:
                    formattedTime = string.Format(format, totalDays);
                    break;
                case DateTimeFormatType.Days_Hours:
                    formattedTime = string.Format(format, totalDays, remainingHours);
                    break;
                case DateTimeFormatType.Days_Hours_Minutes:
                    formattedTime = string.Format(format, totalDays, remainingHours, remainingMinutes);
                    break;
                case DateTimeFormatType.Days_Hours_Minutes_Seconds:
                    formattedTime = string.Format(format, totalDays, remainingHours, remainingMinutes, remainingSeconds);
                    break;
                case DateTimeFormatType.Minutes:
                    formattedTime = string.Format(format, totalMinutes);
                    break;
                case DateTimeFormatType.Minutes_Seconds:
                    formattedTime = string.Format(format, totalMinutes, remainingSeconds);
                    break;
                case DateTimeFormatType.Seconds:
                    formattedTime = string.Format(format, totalSeconds);
                    break;
                case DateTimeFormatType.Default:
                    formattedTime = m_TimeLeft.ToString();
                    break;
                case DateTimeFormatType.None:
                default:
                    format = DATE_TIME_FORMAT; // Default format
                    formattedTime = string.Format(format, totalHours, remainingMinutes, remainingSeconds);
                    break;
            }
            return formattedTime;
        }
        public void ValidateDateTimeFormat(ref DateTimeFormatType currentFormat)
        {
            TimeSpan timeUntilNextDay = m_TimeLeft;

            if (timeUntilNextDay.TotalHours < 1 && timeUntilNextDay.TotalMinutes >= 1)
            {
                currentFormat = DateTimeFormatType.Minutes;
            }
            else if (timeUntilNextDay.TotalMinutes < 1)
            {
                currentFormat = DateTimeFormatType.Seconds;
            }
        }
        public string GetTimeLeftUntilNext_FOR_YOU_LIST_Refresh(DateTimeFormatType dateTimeFormatType = DateTimeFormatType.Hour_Minutes_Seconds)
        {
            TimeSpan timeDifference = m_TimeLeft;
            if (dateTimeFormatType != dateTimeFormat)
                dateTimeFormat = dateTimeFormatType;
            ValidateDateTimeFormat(ref dateTimeFormat);

            return StartFormating(dateTimeFormat, timeDifference);
        }

        public int CalculateDaysPassed(DateTime startDate)
        {
            DateTime currentDate = TryGetSaveDate(out DateTime lastSavedDate) ?
                                   lastSavedDate :
                                   m_CurrentDate;
            TimeSpan timePassed = currentDate - startDate;
            int daysPassed = timePassed.Days;

            return daysPassed;
        }

        #endregion
    }
    [Serializable]
    public struct OnlineDateTimeData
    {
        public string datetime;
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(datetime);
        }
    }
}
