using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(IpadContentSettings))]
public class IpadContentSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        IpadContentSettings settings = (IpadContentSettings)target;

        // IPAD CONTENT Settings
        EditorGUILayout.LabelField("IPAD CONTENT Settings", EditorStyles.boldLabel);
        settings.delaytime = EditorGUILayout.FloatField("Delay Time", settings.delaytime);
        settings.instantChange = EditorGUILayout.Toggle("Instant Change", settings.instantChange);
        settings.forceChange = EditorGUILayout.Toggle("Force Change", settings.forceChange);
        settings.contentTransform = (RectTransform)EditorGUILayout.ObjectField("Content Transform", settings.contentTransform, typeof(RectTransform), true);

        // ANCHOR Settings
        EditorGUILayout.LabelField("ANCHOR Settings", EditorStyles.boldLabel);
        settings.setNewAnchor = EditorGUILayout.Toggle("Set New Anchor", settings.setNewAnchor);
        if (settings.setNewAnchor)
        {
            settings.anchorPreset = (AnchorPresets)EditorGUILayout.EnumPopup("Anchor Preset", settings.anchorPreset);
        }

        // PIVOT Settings
        EditorGUILayout.LabelField("PIVOT Settings", EditorStyles.boldLabel);
        settings.setNewPivot = EditorGUILayout.Toggle("Set New Pivot", settings.setNewPivot);
        if (settings.setNewPivot)
        {
            settings.pivotPreset = (PivotPresets)EditorGUILayout.EnumPopup("Pivot Preset", settings.pivotPreset);
        }

        // POSITION Settings
        EditorGUILayout.LabelField("POSITION Settings", EditorStyles.boldLabel);
        settings.setNewPosition = EditorGUILayout.Toggle("Set New Position", settings.setNewPosition);
        if (settings.setNewPosition)
        {
            settings.leftPosition = EditorGUILayout.FloatField("Left Position", settings.leftPosition);
            settings.rightPosition = EditorGUILayout.FloatField("Right Position", settings.rightPosition);
            settings.topPosition = EditorGUILayout.FloatField("Top Position", settings.topPosition);
            settings.bottomPosition = EditorGUILayout.FloatField("Bottom Position", settings.bottomPosition);
        }

        // SIZE Settings
        EditorGUILayout.LabelField("SIZE Settings", EditorStyles.boldLabel);
        settings.setNewScale = EditorGUILayout.Toggle("Set New Scale", settings.setNewScale);
        if (settings.setNewScale)
        {
            settings.newScale = EditorGUILayout.Vector3Field("New Scale", settings.newScale);
        }

        // POSITION X/Y Settings
        EditorGUILayout.LabelField("POSITION X/Y Settings", EditorStyles.boldLabel);
        settings.setNewPosX = EditorGUILayout.Toggle("Set New PosX", settings.setNewPosX);
        if (settings.setNewPosX)
        {
            settings.posX = EditorGUILayout.FloatField("PosX", settings.posX);
        }
        settings.setNewPosY = EditorGUILayout.Toggle("Set New PosY", settings.setNewPosY);
        if (settings.setNewPosY)
        {
            settings.posY = EditorGUILayout.FloatField("PosY", settings.posY);
        }
        settings.setNewPosZ = EditorGUILayout.Toggle("Set New PosZ", settings.setNewPosZ);
        if (settings.setNewPosZ)
        {
            settings.posZ = EditorGUILayout.FloatField("PosZ", settings.posZ);
        }

        // LAYOUT MODIFIERS
        EditorGUILayout.LabelField("LAYOUT MODIFIERS", EditorStyles.boldLabel);
        settings.disableVerticalLayout = EditorGUILayout.Toggle("Disable Vertical Layout", settings.disableVerticalLayout);
        if (settings.disableVerticalLayout)
        {
            settings.verticalLayoutPadding = EditorGUILayout.FloatField("Vertical Layout Padding", settings.verticalLayoutPadding);
        }
        settings.disableHorizontalLayout = EditorGUILayout.Toggle("Disable Horizontal Layout", settings.disableHorizontalLayout);
        if (settings.disableHorizontalLayout)
        {
            settings.horizontalLayoutPadding = EditorGUILayout.FloatField("Horizontal Layout Padding", settings.horizontalLayoutPadding);
        }

        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(settings);
        }
    }
}
#endif