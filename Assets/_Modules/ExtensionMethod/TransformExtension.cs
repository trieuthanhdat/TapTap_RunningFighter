using UnityEngine;

public static class TransformExtension
{
    public static void SetLocalX(this Transform me, float x)
    {
        Vector3 pos = me.localPosition;
        pos.x = x;
        me.localPosition = pos;
    }

    public static void SetLocalY(this Transform me, float y)
    {
        Vector3 pos = me.localPosition;
        pos.y = y;
        me.localPosition = pos;
    }

    public static void SetLocalZ(this Transform me, float z)
    {
        Vector3 pos = me.localPosition;
        pos.z = z;
        me.localPosition = pos;
    }

    public static void SetLocalPosition(this Transform me, float x, float y)
    {
        Vector3 pos = me.localPosition;
        pos.x = x;
        pos.y = y;
        me.localPosition = pos;
    }

    public static void SetXYPosition(this Transform me, float x, float y)
    {
        Vector3 pos = me.position;
        pos.x = x;
        pos.y = y;
        me.position = pos;
    }

    public static void SetX(this Transform me, float x)
    {
        Vector3 pos = me.position;
        pos.x = x;
        me.position = pos;
    }

    public static void SetY(this Transform me, float y)
    {
        Vector3 pos = me.position;
        pos.y = y;
        me.position = pos;
    }

    public static void AddY(this Transform me, float y)
    {
        Vector3 pos = me.position;
        pos.y += y;
        me.position = pos;
    }

    public static void AddX(this Transform me, float x)
    {
        Vector3 pos = me.position;
        pos.x += x;
        me.position = pos;
    }
}