using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScaleScreenType
{
    FullHD,
    Ipad,
    Promax
}


public class DetectScreenSize : MonoSingleton<DetectScreenSize>
{
    [SerializeField] ScaleScreenType scaleScreenType;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);

        detectScreenSize();
    }

    public ScaleScreenType GetScreenType()
    {
        return scaleScreenType;
    }


    void detectScreenSize()
    {
        float aspectRatio = Mathf.Max(Screen.width, Screen.height) * 1f / Mathf.Min(Screen.width, Screen.height);
        if (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f)
            scaleScreenType = ScaleScreenType.Ipad;

        if (DeviceDiagonalSizeInInches() >= 6f && aspectRatio >= 2f)
            scaleScreenType = ScaleScreenType.Promax;

        if (aspectRatio >= 1.7f && aspectRatio <= 1.8)
            scaleScreenType = ScaleScreenType.FullHD;
    }


    private float DeviceDiagonalSizeInInches()
    {
        float screenWidth = Screen.width * 1f / Screen.dpi;
        float screenHeight = Screen.height * 1f / Screen.dpi;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

        return diagonalInches;
    }

}

