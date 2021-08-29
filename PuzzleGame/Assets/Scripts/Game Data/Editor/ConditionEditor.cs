using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PuzzleGame.Editor
{
    [CustomEditor(typeof(Condition))]
    public class ConditionEditor : UnityEditor.Editor
    {
        Condition _target;

        private void OnEnable()
        {
            _target = target as Condition;
        }

        public override void OnInspectorGUI()
        {
            if(_target && _target.expression != null)
                EditorGUILayout.LabelField("expression: " + string.Join(" ", _target.expression));

            DrawDefaultInspector();
        }
    }
}
