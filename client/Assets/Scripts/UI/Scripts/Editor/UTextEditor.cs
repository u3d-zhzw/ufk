using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using System;
using System.Reflection;
using System.Security.Policy;

namespace UFK
{
    [CustomEditor(typeof(UText), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Text Component.
    ///   Extend this class to write a custom editor for an Text-derived component.
    /// </summary>
    public class UTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty m_Text;
        SerializedProperty m_UFontData;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");
            m_UFontData = serializedObject.FindProperty("m_UFontData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_UFontData);
            
            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
