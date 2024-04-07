using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_RunningFighter.Data
{
    [CreateAssetMenu(menuName = "GameData/NameGeneration", order = 2)]
    public class NameGenerationData : ScriptableObject
    {
        [Tooltip("The list of all possible strings the game can use as the first word of a player name")]
        public string[] FirstWordList;

        [Tooltip("The list of all possible strings the game can use as the second word in a player name")]
        public string[] SecondWordList;

        public string GenerateName()
        {
            var firstWord = FirstWordList[Random.Range(0, FirstWordList.Length - 1)];
            var secondWord = SecondWordList[Random.Range(0, SecondWordList.Length - 1)];

            return firstWord + " " + secondWord;
        }
    }
}
