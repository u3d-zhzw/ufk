using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;

namespace UFK
{
    [CustomEditor(typeof(UIImage), true)]
    [CanEditMultipleObjects]
    public class UIImageEditor : ImageEditor
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