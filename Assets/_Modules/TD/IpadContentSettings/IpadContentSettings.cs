using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IpadContentSettings : MonoBehaviour
{
    [Header("IPAD CONTENT Settings")]
    public float delaytime = 0.5f;
    public bool instantChange = false;
    public bool forceChange = false;
    public RectTransform contentTransform;

    [Header("ANCHOR Settings")]
    public bool setNewAnchor = false;
    public AnchorPresets anchorPreset;

    [Header("PIVOT Settings")]
    public bool setNewPivot = false;
    public PivotPresets pivotPreset;

    [Header("POSITION Settings")]
    public bool setNewPosition = false;
    public float leftPosition;
    public float rightPosition;
    public float topPosition;
    public float bottomPosition;

    [Header("SIZE Settings")]
    public bool setNewScale = false;
    public Vector3 newScale;

    [Header("POSITION X/Y Settings")]
    public bool setNewPosX = false;
    public float posX;
    public bool setNewPosY = false;
    public float posY;
    public bool setNewPosZ = false;
    public float posZ;

    [Header("LAYOUT MODIFIERS")]
    public bool disableVerticalLayout = false;
    public bool disableHorizontalLayout = false;
    public float verticalLayoutPadding;
    public float horizontalLayoutPadding;
    protected bool _isIpad = false;
    private void OnEnable()
    {
        _isIpad = RectTransformExtensionsSettings.IsIpadOrTablet();
        if (!_isIpad) return;

        if (instantChange)
        {
            ApplySettingsInstantly();
        }
        else
        {
            Invoke(nameof(ApplySettingsDelayed), delaytime);
        }
    }

    private void Update()
    {
        if (!_isIpad) return;
        if (forceChange)
        {
            ApplySettingsInstantly();
        }
    }

    public void ApplySettingsInstantly()
    {
        ApplyAnchorSettings();
        ApplyPivotSettings();
        ApplyPositionSettings();
        ApplyScaleSettings();
        ApplyLayoutModifiers();
    }

    public void ApplySettingsDelayed()
    {
        StartCoroutine(ApplyAnchorSettingsDelayed());
        StartCoroutine(ApplyPivotSettingsDelayed());
        StartCoroutine(ApplyPositionSettingsDelayed());
        StartCoroutine(ApplyScaleSettingsDelayed());
        StartCoroutine(ApplyLayoutModifiersDelayed());
    }

    private void ApplyAnchorSettings()
    {
        if (setNewAnchor)
        {
            RectTransformExtensionsSettings.SetAnchor(contentTransform, anchorPreset);
        }
    }

    private void ApplyPivotSettings()
    {
        if (setNewPivot)
        {
            RectTransformExtensionsSettings.SetPivot(contentTransform, pivotPreset);
        }
    }
    #region ____RECT POSITIONS____
    private void ApplyPositionSettings()
    {
        if (setNewPosition)
        {
            // Apply position settings based on the current anchor and pivot presets
            if (!setNewAnchor)
            {
                // Get the current anchor settings from RectTransform
                AnchorPresets currentAnchorPreset = RectTransformExtensionsSettings.GetCurrentAnchorPreset(contentTransform.anchorMin, contentTransform.anchorMax);
                Debug.Log($"IPAD CONTENT SETTINGS: currentAnchorPreset {currentAnchorPreset}");
                // Apply position settings based on the detected anchor preset
                ApplyPositionBasedOnAnchorPreset(currentAnchorPreset);
            }
            else
            {
                // Apply position settings based on the specified anchor preset
                ApplyPositionBasedOnAnchorPreset(anchorPreset);
            }
        }
        // Apply individual position settings if flags are set
        ApplyIndividualPositionSettings();
    }

    private void ApplyIndividualPositionSettings()
    {
        Vector3 newPosition = contentTransform.anchoredPosition;
        if (setNewPosX)
        {
            newPosition.x = posX;
            contentTransform.anchoredPosition = newPosition;
        }
        if (setNewPosY)
        {
            newPosition.y = posY;
            contentTransform.anchoredPosition = newPosition;
        }
        if (setNewPosZ)
        {
            newPosition.z = posZ;
            contentTransform.anchoredPosition = newPosition;
        }
    }

    private void ApplyPositionBasedOnAnchorPreset(AnchorPresets preset)
    {
        switch (preset)
        {
            case AnchorPresets.TopLeft:
                ApplyPositionTopLeft();
                break;
            case AnchorPresets.TopCenter:
                ApplyPositionTopCenter();
                break;
            case AnchorPresets.TopRight:
                ApplyPositionTopRight();
                break;

            case AnchorPresets.MiddleLeft:
                ApplyPositionMiddleLeft();
                break;
            case AnchorPresets.MiddleCenter:
                ApplyPositionMiddleCenter();
                break;
            case AnchorPresets.MiddleRight:
                ApplyPositionMiddleRight();
                break;

            case AnchorPresets.BottomLeft:
                ApplyPositionBottomLeft();
                break;
            case AnchorPresets.BottonCenter:
                ApplyPositionBottomCenter();
                break;
            case AnchorPresets.BottomRight:
                ApplyPositionBottomRight();
                break;

            case AnchorPresets.HorStretchTop:
                ApplyPositionHorStretchTop();
                break;
            case AnchorPresets.HorStretchMiddle:
                ApplyPositionHorStretchMiddle();
                break;
            case AnchorPresets.HorStretchBottom:
                ApplyPositionHorStretchBottom();
                break;

            case AnchorPresets.VertStretchLeft:
                ApplyPositionVertStretchLeft();
                break;
            case AnchorPresets.VertStretchCenter:
                ApplyPositionVertStretchCenter();
                break;
            case AnchorPresets.VertStretchRight:
                ApplyPositionVertStretchRight();
                break;

            case AnchorPresets.StretchAll:
                ApplyPositionStretchAll();
                break;
        }
    }

    private void ApplyPositionTopLeft()
    {
        contentTransform.anchoredPosition = new Vector2(leftPosition, -topPosition);
    }

    private void ApplyPositionTopCenter()
    {
        contentTransform.anchoredPosition = new Vector2(posX, -topPosition);
    }

    private void ApplyPositionTopRight()
    {
        contentTransform.anchoredPosition = new Vector2(-rightPosition, -topPosition);
    }

    private void ApplyPositionMiddleLeft()
    {
        contentTransform.anchoredPosition = new Vector2(leftPosition, posY);
    }

    private void ApplyPositionMiddleCenter()
    {
        contentTransform.anchoredPosition = new Vector2(posX, posY);
    }

    private void ApplyPositionMiddleRight()
    {
        contentTransform.anchoredPosition = new Vector2(-rightPosition, posY);
    }

    private void ApplyPositionBottomLeft()
    {
        contentTransform.anchoredPosition = new Vector2(leftPosition, bottomPosition);
    }

    private void ApplyPositionBottomCenter()
    {
        contentTransform.anchoredPosition = new Vector2(posX, bottomPosition);
    }

    private void ApplyPositionBottomRight()
    {
        contentTransform.anchoredPosition = new Vector2(-rightPosition, bottomPosition);
    }

    private void ApplyPositionHorStretchTop()
    {
        contentTransform.anchoredPosition = new Vector2(posX, -topPosition);
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, bottomPosition - topPosition);
    }

    private void ApplyPositionHorStretchMiddle()
    {
        contentTransform.anchoredPosition = new Vector2(posX, posY);
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, bottomPosition - topPosition);
    }

    private void ApplyPositionHorStretchBottom()
    {
        contentTransform.anchoredPosition = new Vector2(posX, bottomPosition);
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, -topPosition);
    }

    private void ApplyPositionVertStretchLeft()
    {
        contentTransform.anchoredPosition = new Vector2(leftPosition, posY);
        contentTransform.sizeDelta = new Vector2(rightPosition - leftPosition, contentTransform.sizeDelta.y);
    }

    private void ApplyPositionVertStretchCenter()
    {
        contentTransform.anchoredPosition = new Vector2(posX, posY);
        contentTransform.sizeDelta = new Vector2(rightPosition - leftPosition, contentTransform.sizeDelta.y);
    }

    private void ApplyPositionVertStretchRight()
    {
        contentTransform.anchoredPosition = new Vector2(rightPosition, posY);
        contentTransform.sizeDelta = new Vector2(-leftPosition, contentTransform.sizeDelta.y);
    }

    private void ApplyPositionStretchAll()
    {
        contentTransform.anchoredPosition = new Vector2(posX, posY);
        contentTransform.sizeDelta = new Vector2(rightPosition - leftPosition, bottomPosition - topPosition);
    }
    #endregion
    private void ApplyScaleSettings()
    {
        if (setNewScale)
        {
            contentTransform.localScale = newScale;
        }
    }

    private void ApplyLayoutModifiers()
    {
        if (disableVerticalLayout)
        {
            var verticalLayout = contentTransform.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                verticalLayout.enabled = false;
            }
        }

        if (disableHorizontalLayout)
        {
            var horizontalLayout = contentTransform.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                horizontalLayout.enabled = false;
            }
        }
    }

    private IEnumerator ApplyAnchorSettingsDelayed()
    {
        yield return new WaitForSeconds(delaytime);
        ApplyAnchorSettings();
    }

    private IEnumerator ApplyPivotSettingsDelayed()
    {
        yield return new WaitForSeconds(delaytime);
        ApplyPivotSettings();
    }

    private IEnumerator ApplyPositionSettingsDelayed()
    {
        yield return new WaitForSeconds(delaytime);
        ApplyPositionSettings();
    }

    private IEnumerator ApplyScaleSettingsDelayed()
    {
        yield return new WaitForSeconds(delaytime);
        ApplyScaleSettings();
    }

    private IEnumerator ApplyLayoutModifiersDelayed()
    {
        yield return new WaitForSeconds(delaytime);
        ApplyLayoutModifiers();
    }
}
