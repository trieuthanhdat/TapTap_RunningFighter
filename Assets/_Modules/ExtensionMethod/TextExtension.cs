using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TextExtension
{
    public static void SetAlpha(this Text me, float alpha)
    {
        Color color = me.color;
        color.a = alpha;
        me.color = color;
    }
}
