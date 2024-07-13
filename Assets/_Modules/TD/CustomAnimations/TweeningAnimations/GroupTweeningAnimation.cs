using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroupTweeningAnimation
{
    public List<TweeningAnimation> tweeningAnimations;
    public SequentialAnimationType sequentialAnimationType;
    public int loopTime = 1;
    public float waitTimeBetweenAnimations = 0.1f;
    public bool useCoroutine = false;

    public GroupTweeningAnimation()
    {
        tweeningAnimations = new List<TweeningAnimation>();
        sequentialAnimationType = SequentialAnimationType.Sequential;
    }
}
