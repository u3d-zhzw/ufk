using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace UFK
{
    [CustomEditor(typeof(UImage), true)]
    [CanEditMultipleObjects]
    public class UImageEditor : ImageEditor
    {
        protected SerializedProperty m_HideWhenNoneSprite;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_HideWhenNoneSprite = serializedObject.FindProperty("m_HideWhenNoneSprite");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_HideWhenNoneSprite);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}