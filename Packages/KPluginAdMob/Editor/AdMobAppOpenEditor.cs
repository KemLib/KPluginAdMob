using UnityEditor;
using UnityEngine;

namespace KPlugin.AdMob.Editor
{

    [CustomEditor(typeof(AdMobAppOpen))]
    public class AdMobAppOpenEditor : UnityEditor.Editor
    {
        #region Properties
        private SerializedProperty propertyInitIndispensable,
            propertySetInstance,
            propertyIsAutoReload,
            propertyAdName,
            propertyIndexAd;
        #endregion

        #region Methods Unity        
        private void OnEnable()
        {
            propertyAdName = serializedObject.FindProperty("adName");
            propertyIsAutoReload = serializedObject.FindProperty("isAutoReload");
            propertySetInstance = serializedObject.FindProperty("setInstance");
            propertyInitIndispensable = serializedObject.FindProperty("initIndispensable");
            propertyIndexAd = serializedObject.FindProperty("indexAd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //
            EditorGUILayout.PropertyField(propertyInitIndispensable, new GUIContent("Init Indispensable"));
            EditorGUILayout.PropertyField(propertySetInstance, new GUIContent("Set Instance"));
            EditorGUILayout.PropertyField(propertyAdName, new GUIContent("Ad Name"));
            if (string.IsNullOrEmpty(propertyAdName.stringValue))
            {
                propertyAdName.stringValue = serializedObject.targetObject.name;
            }
            EditorGUILayout.PropertyField(propertyIsAutoReload, new GUIContent("Auto Reload"));
            EditorGUILayout.PropertyField(propertyIndexAd, new GUIContent("Index Ad"));
            //
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Methods

        #endregion
    }
}
