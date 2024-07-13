using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanotes
{
    public class TweeningGroupItem : MonoBehaviour
    {
        
        [SerializeField] List<GroupTweeningItemClass> listGroupTweeningItem = new List<GroupTweeningItemClass>();
        [SerializeField] private bool PlayOnAwake = false;
        [SerializeField] private float delayTime = 0.1f;
        private int itemPlayedCounter = 0;
        public static event Action OnLastItemPlayed;
        private void Start()
        {
            if (PlayOnAwake)
            {
                StartCoroutine(PlayGroupAnimation());
            }
        }
        public void AddGroupTweeningItem(GroupTweeningItemClass group, bool renew = true)
        {
            if(renew)
            {
                listGroupTweeningItem.Clear();
            }
            listGroupTweeningItem.Add(group);
        }

        
        //=====SEQUENCE CONTROL=====//
        #region SEQUENCE CONTROL

        private IEnumerator PlayGroupAnimation()
        {
            yield return new WaitForSeconds(delayTime);
            if (listGroupTweeningItem == null) yield break;
            DOTween.KillAll();
            foreach(var group in listGroupTweeningItem)
            {
                switch (group.groupTweeningType)
                {
                    case GroupTweeningItemType.SEQUENCE:
                        if (group.listTweeningItem == null) continue;
                        float timeInterval = group.sequenceTimeBetweenElements;
                        itemPlayedCounter = 0;

                        StartCoroutine(SequenceTweeningItem(group, timeInterval));
                        break;
                    case GroupTweeningItemType.ONETIME:
                        if (group.listTweeningItem == null) continue;
                        Sequence sequenceOneTime = DOTween.Sequence();
                        float delay = group.sequenceTimeBetweenElements;
                        foreach (var item in group.listTweeningItem)
                        {
                            sequenceOneTime.Join(item.GetItemTween(item.gameObject, item.OriginScale, item.OriginPosition, item.tweeningType)).SetDelay(delay);
                        }
                        sequenceOneTime.Play();
                        break;
                }
            }
        }

        private IEnumerator SequenceTweeningItem(GroupTweeningItemClass group, float timeInterval)
        {
            Debug.Log("TWEENING GROUP ITEM: counter item "+ itemPlayedCounter);
            foreach (var item in group.listTweeningItem)
            {
                item.GetItemTween(item.gameObject, item.OriginScale, item.OriginPosition, item.tweeningType).Play();
                itemPlayedCounter++;
                //MonoAudioManager.instance.PlaySound("ButtonPopV3");
                yield return new WaitForSeconds(timeInterval);
            }
            if(itemPlayedCounter >= group.listTweeningItem.Count)
            {
                OnLastItemPlayed?.Invoke();
            }
        }
        #endregion
    }
    [Serializable]
    public class GroupTweeningItemClass
    {
        public GroupTweeningItemType groupTweeningType = GroupTweeningItemType.NONE;
        public List<TweeningItem> listTweeningItem = new List<TweeningItem>();
        public float sequenceTimeBetweenElements = 0;
        public GroupTweeningItemClass(GroupTweeningItemType type, List<TweeningItem> listItem, float timeDelay)
        {
            groupTweeningType = type;
            listTweeningItem = listItem;
            sequenceTimeBetweenElements = timeDelay;
        }
    }
    public enum GroupTweeningItemType
    {
        NONE,
        SEQUENCE,
        ONETIME,
    }
}
