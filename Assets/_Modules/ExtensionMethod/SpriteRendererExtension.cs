using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteRendererExtension
{
    public static void SetSizeX(this SpriteRenderer me, float newSizeX)
    {
        Vector3 scale = me.transform.localScale;

        float currentSizeX = me.bounds.extents.x * 2f;
        float newScaleX = newSizeX * scale.x / currentSizeX;

        scale.x = newScaleX;
        me.transform.localScale = scale;
    }

    public static void SetSizeY(this SpriteRenderer me, float newSizeY)
    {
        Vector3 scale;
        float currentSizeY = me.bounds.extents.y * 2f;
        if (me.drawMode == SpriteDrawMode.Sliced || me.drawMode == SpriteDrawMode.Tiled)
        {
            // set he so sliced
            scale = me.size;
        }
        else
        {
            // chinh he so scale local
            scale = me.transform.localScale;
        }

        float factor = newSizeY / currentSizeY;
        float newScaleY = factor * scale.y;

        scale.y = newScaleY;

        if (me.drawMode == SpriteDrawMode.Sliced || me.drawMode == SpriteDrawMode.Tiled)
        {
            me.size = scale;
        }
        else
        {
            me.transform.localScale = scale;
        }
    }

    public static void SetSize(this SpriteRenderer me, float newSizeX, float newSizeY)
    {
        Vector3 scale = me.transform.localScale;
        Vector3 extents = me.bounds.extents;
        
        if (me.drawMode == SpriteDrawMode.Sliced || me.drawMode == SpriteDrawMode.Tiled)
        {
            // set he so sliced
            scale = me.size;
        }
        else
        {
            scale = me.transform.localScale;
        }
        
        // set x
        float currentSizeX = extents.x * 2f;
        float newScaleX = newSizeX * scale.x / currentSizeX;
        scale.x = newScaleX;
        
        // set y
        float currentSizeY = extents.y * 2f;
        float factor = newSizeY / currentSizeY;
        float newScaleY = factor * scale.y;
        scale.y = newScaleY;

        if (me.drawMode == SpriteDrawMode.Sliced || me.drawMode == SpriteDrawMode.Tiled)
        {
            me.size = scale;
        }
        else
        {
            me.transform.localScale = scale;
        }
    }

    public static void SetSize(this SpriteRenderer me, Vector2 size)
    {
        me.SetSize(size.x, size.y);
    }

    public static float GetSizeY(this SpriteRenderer me)
    {
        float retVal = 0f;

        if (me.drawMode == SpriteDrawMode.Sliced)
        {
            retVal = me.size.y * me.transform.localScale.y;
        }
        else
        {
            retVal = me.bounds.extents.y * 2f;
        }

        return retVal;
    }
    
    public static void SetAlpha(this SpriteRenderer me, float alpha) {
        Color color = me.color;
        color.a = alpha;
        me.color = color;
    }
}