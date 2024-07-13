using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}

public static class RectTransformExtensionsSettings
{
    public static AnchorPresets GetCurrentAnchorPreset(Vector2 anchorMin, Vector2 anchorMax)
    {
        if (anchorMin == new Vector2(0, 1) && anchorMax == new Vector2(0, 1))
            return AnchorPresets.TopLeft;
        if (anchorMin == new Vector2(0.5f, 1) && anchorMax == new Vector2(0.5f, 1))
            return AnchorPresets.TopCenter;
        if (anchorMin == new Vector2(1, 1) && anchorMax == new Vector2(1, 1))
            return AnchorPresets.TopRight;

        if (anchorMin == new Vector2(0, 0.5f) && anchorMax == new Vector2(0, 0.5f))
            return AnchorPresets.MiddleLeft;
        if (anchorMin == new Vector2(0.5f, 0.5f) && anchorMax == new Vector2(0.5f, 0.5f))
            return AnchorPresets.MiddleCenter;
        if (anchorMin == new Vector2(1, 0.5f) && anchorMax == new Vector2(1, 0.5f))
            return AnchorPresets.MiddleRight;

        if (anchorMin == new Vector2(0, 0) && anchorMax == new Vector2(0, 0))
            return AnchorPresets.BottomLeft;
        if (anchorMin == new Vector2(0.5f, 0) && anchorMax == new Vector2(0.5f, 0))
            return AnchorPresets.BottonCenter; // Note the typo in BottonCenter
        if (anchorMin == new Vector2(1, 0) && anchorMax == new Vector2(1, 0))
            return AnchorPresets.BottomRight;

        if (anchorMin == new Vector2(0, 1) && anchorMax == new Vector2(1, 1))
            return AnchorPresets.HorStretchTop;
        if (anchorMin == new Vector2(0, 0.5f) && anchorMax == new Vector2(1, 0.5f))
            return AnchorPresets.HorStretchMiddle;
        if (anchorMin == new Vector2(0, 0) && anchorMax == new Vector2(1, 0))
            return AnchorPresets.HorStretchBottom;

        if (anchorMin == new Vector2(0, 0) && anchorMax == new Vector2(0, 1))
            return AnchorPresets.VertStretchLeft;
        if (anchorMin == new Vector2(0.5f, 0) && anchorMax == new Vector2(0.5f, 1))
            return AnchorPresets.VertStretchCenter;
        if (anchorMin == new Vector2(1, 0) && anchorMax == new Vector2(1, 1))
            return AnchorPresets.VertStretchRight;

        if (anchorMin == new Vector2(0, 0) && anchorMax == new Vector2(1, 1))
            return AnchorPresets.StretchAll;

        // Default case
        return AnchorPresets.MiddleCenter;
    }

    public static void SetAnchor(RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
    {
        source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (allign)
        {
            case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
        }
    }

    public static void SetPivot(RectTransform source, PivotPresets preset)
    {

        switch (preset)
        {
            case (PivotPresets.TopLeft):
                {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
            case (PivotPresets.TopCenter):
                {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
            case (PivotPresets.TopRight):
                {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

            case (PivotPresets.MiddleLeft):
                {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleCenter):
                {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleRight):
                {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

            case (PivotPresets.BottomLeft):
                {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
            case (PivotPresets.BottomCenter):
                {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
            case (PivotPresets.BottomRight):
                {
                    source.pivot = new Vector2(1, 0);
                    break;
                }
        }
    }

    public static bool IsIpadOrTablet()
    {
        var identifier = SystemInfo.deviceModel;
        Debug.Log($"RECT TRANS EXTENTION SETTINGS: identifier {identifier}");
#if UNITY_IOS && !UNITY_EDITOR//IOS 
        if (identifier.StartsWith("iPhone"))
        {
            // iPhone logic
            return false;
        }
        else if (identifier.StartsWith("iPad") || identifier.StartsWith("Apple iPad"))
        {
            // iPad logic
            Debug.Log("RECT TRANS EXTENSION SETTINGS: IN IPAD SCREEN !!");
            return true;
        }
        else
        {
            // Mac logic?
            return false;
        }
#else   //ANDROID OR OTHERS
        var scaleScreenType = DetectScreenSize.Instance.GetScreenType();
        Debug.Log("RECT TRANS EXTENSION SETTINGS: Screen type !!" + scaleScreenType);
        return scaleScreenType == ScaleScreenType.Ipad;
#endif
    }
}