using KTool.Advertisement;
using UnityEditor;
using UnityEngine;

namespace KPlugin.GoogleAdMob.Editor
{

    [CustomEditor(typeof(AdMobBanner))]
    public class AdMobBannerEditor : UnityEditor.Editor
    {
        #region Properties
        private SerializedProperty propertyAdName,
            propertyIsAutoReload,
            propertyAdPosition,
            propertyAdSize,
            propertyPosition,
            propertySize,
            propertySetInstance,
            propertyInitIndispensable,
            propertyIndexAd;
        private AdPosition[] listAdPosition = new AdPosition[] { AdPosition.Custom, AdPosition.TopLeft, AdPosition.TopCenter, AdPosition.TopRight, AdPosition.MidCenter, AdPosition.BotLeft, AdPosition.BotCenter, AdPosition.BotRight };
        private string[] listAdPositionString = new string[] { "Custom", "TopLeft", "Top", "TopRight", "Center", "BottomLeft", "Bottom", "BottomRight" };
        private int indexAdPosition;
        private AdSize[] listAdSize = new AdSize[] { AdSize.Custom, AdSize.Standard, AdSize.Large, AdSize.Medium, AdSize.FullSize };
        private string[] listAdSizeString = new string[] { "Custom", "Banner", "Medium Rectangle", "IABBanner", "Leaderboard" };
        private int indexAdSize;
        #endregion

        #region Methods Unity        
        private void OnEnable()
        {
            propertyAdName = serializedObject.FindProperty("adName");
            propertyIsAutoReload = serializedObject.FindProperty("isAutoReload");
            propertyAdPosition = serializedObject.FindProperty("adPosition");
            propertyAdSize = serializedObject.FindProperty("adSize");
            propertyPosition = serializedObject.FindProperty("position");
            propertySize = serializedObject.FindProperty("size");
            propertySetInstance = serializedObject.FindProperty("setInstance");
            propertyInitIndispensable = serializedObject.FindProperty("initIndispensable");
            propertyIndexAd = serializedObject.FindProperty("indexAd");
            //
            indexAdPosition = -1;
            for (int i = 0; i < listAdPosition.Length; i++)
                if ((int)listAdPosition[i] == propertyAdPosition.enumValueIndex)
                {
                    indexAdPosition = i;
                    break;
                }
            //
            indexAdSize = -1;
            for (int i = 0; i < listAdSize.Length; i++)
                if ((int)listAdSize[i] == propertyAdSize.enumValueIndex)
                {
                    indexAdSize = i;
                    break;
                }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //
            EditorGUILayout.PropertyField(propertySetInstance, new GUIContent("Set Instance"));
            EditorGUILayout.PropertyField(propertyInitIndispensable, new GUIContent("Init Indispensable"));
            EditorGUILayout.PropertyField(propertyAdName, new GUIContent("Ad Name"));
            if (string.IsNullOrEmpty(propertyAdName.stringValue))
            {
                propertyAdName.stringValue = serializedObject.targetObject.name;
            }
            EditorGUI.BeginDisabledGroup(true);
            if (!propertyIsAutoReload.boolValue)
                propertyIsAutoReload.boolValue = true;
            EditorGUILayout.PropertyField(propertyIsAutoReload, new GUIContent("Auto Reload"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(propertyIndexAd, new GUIContent("Index Ad"));
            //
            if (indexAdPosition == -1)
            {
                indexAdPosition = 6;
                propertyAdPosition.enumValueIndex = (int)listAdPosition[indexAdPosition];
            }
            int newIndexAdPosition = EditorGUILayout.Popup(new GUIContent("Ad Position"), indexAdPosition, listAdPositionString);
            if (newIndexAdPosition != indexAdPosition)
            {
                indexAdPosition = newIndexAdPosition;
                propertyAdPosition.enumValueIndex = (int)listAdPosition[indexAdPosition];
            }
            if (propertyAdPosition.enumValueIndex == (int)AdPosition.Custom)
            {
                EditorGUILayout.PropertyField(propertyPosition, new GUIContent("Position"));
                Vector2 size = propertyPosition.vector2Value;
                propertyPosition.vector2Value = new Vector2(Mathf.Max(0, size.x), Mathf.Max(0, size.y));
            }
            //
            if (indexAdSize == -1)
            {
                indexAdSize = 0;
                propertyAdSize.enumValueIndex = (int)listAdSize[indexAdSize];
            }
            int newIndexAdSize = EditorGUILayout.Popup(new GUIContent("Ad Size"), indexAdSize, listAdSizeString);
            if (newIndexAdSize != indexAdSize)
            {
                indexAdSize = newIndexAdSize;
                propertyAdSize.enumValueIndex = (int)listAdSize[indexAdSize];
            }
            if (propertyAdSize.enumValueIndex == (int)AdSize.Custom)
            {
                EditorGUILayout.PropertyField(propertySize, new GUIContent("Size"));
                Vector2 size = propertySize.vector2Value;
                propertySize.vector2Value = new Vector2(Mathf.Max(0, size.x), Mathf.Max(0, size.y));
            }
            //
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Methods

        #endregion
    }
}
