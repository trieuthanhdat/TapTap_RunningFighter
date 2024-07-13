using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ImageExtension 
{
    public static void SetAlpha(this Image me, float alpha)
    {
        Color color = me.color;
        color.a = alpha;
        me.color = color;
    }
}
