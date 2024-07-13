using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupDailyReward : BaseUI
{
    [Header("DAILY REWARD REFERENCES")]
    [SerializeField] private DailyRewardUIItem pfRewardUIItem;
    [SerializeField] private DailyRewardUIItem pfRewardUIItemBig;
    [SerializeField] private Transform tfContent;   //SpawnPosition of the rewards
    [SerializeField] private Transform tfBigContent; //SpawnPosition of the 7th reward

    private List<DailyRewardUIItem> listInitedUIItems = new List<DailyRewardUIItem>();
    private List<DailyRewardItem> listDailyRewardItem = new List<DailyRewardItem>();

    DailyRewardManager _dailyRewardManager;
    private bool _isInited = false;
    private void Awake()
    {
        _dailyRewardManager = DailyRewardManager.Instance;
    }
    private void OnEnable()
    {
        DailyRewardManager.OnRewardItemClaimedSuccessfully += DailyRewardManager_OnRewardItemClaimedSuccessfully;
    }
    private void OnDisable()
    {
        DailyRewardManager.OnRewardItemClaimedSuccessfully -= DailyRewardManager_OnRewardItemClaimedSuccessfully;
    }

    private void DailyRewardManager_OnRewardItemClaimedSuccessfully()
    {
        InitItems();
    }

    public override void ShowPopup(bool smooth = true)
    {
        base.ShowPopup();
        InitItems();
    }

    private void InitItems()
    {
        if (_dailyRewardManager == null) return;

        listDailyRewardItem = _dailyRewardManager.DailyRewardDataConfigs.listDailyReward;

        if(listInitedUIItems.Count != listDailyRewardItem.Count)
        {
            ClearAllInitedUIItem();
            _isInited = false;
        }

        if(!_isInited)
        {
            for (int i = 0; i < listDailyRewardItem.Count; i++)
            {
                DailyRewardItem item = listDailyRewardItem[i];
                Transform positionToInstantiate = item.isBigItem ? tfBigContent : tfContent;
                DailyRewardUIItem prefab = item.isBigItem ? pfRewardUIItemBig : pfRewardUIItem;
                DailyRewardUIItem UIItem = Instantiate(prefab, positionToInstantiate);

                UIItem.Refresh(item, GetRewardItemClaimableStatus(item));
                listInitedUIItems.Add(UIItem);
            }
            _isInited = true;

        }
        else
        {
            for(int i = 0;i < listInitedUIItems.Count;i++)
            {
                DailyRewardItem item = listDailyRewardItem[i];
                listInitedUIItems[i].Refresh(item, GetRewardItemClaimableStatus(item));
            }
        }
    }
    private void ClearAllInitedUIItem()
    {
        var listTemp = listInitedUIItems;
        for (int i = 0;i < listTemp.Count;i++)
        {
            Destroy(listInitedUIItems[i]);
        }
        listInitedUIItems.Clear();
        listInitedUIItems = new List<DailyRewardUIItem>();
    }
    public DailyRewardClaimableStatus GetRewardItemClaimableStatus(DailyRewardItem item)
    {
        return DailyRewardManager.Instance.GetRewardItemClaimableStatus(item);
    }
}
