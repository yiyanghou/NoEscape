using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PuzzleGame
{
    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        const int k_numLanguages = (int)ELanguageType._MAX;
        const float k_labelHeight = 15f;
        const float k_lineHeight = 35f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect r = new Rect(position.x, position.y, position.width, k_labelHeight);
            EditorGUI.LabelField(r, label);

            float curY = position.y + k_labelHeight;
            for (int i=0; i<k_numLanguages; ++i)
            {
                SerializedProperty prop = property.FindPropertyRelative("_locMapping").GetArrayElementAtIndex(i);

                r = new Rect(position.x, curY, position.width, k_lineHeight);
                r = EditorGUI.PrefixLabel(r, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(((ELanguageType)i).ToString()));
                prop.stringValue = EditorGUI.TextArea(r, "");

                curY += k_lineHeight;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return k_labelHeight + k_numLanguages * k_lineHeight;
        }
    }
}