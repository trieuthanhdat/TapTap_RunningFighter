using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtension 
{
    public static List<T> FromJsonToList<T>(this string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        if (wrapper == null || wrapper.items == null) return null;
        return wrapper.items;
    }
    [System.Serializable]
    public class Wrapper<T>
    {
        public List<T> items;
    }
}
