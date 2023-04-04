using BoneTool.Script.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BoneTool.Script.Editor
{
    [CustomEditor(typeof(BonePoseLib))]
    public class BonePoseLibEditor : UnityEditor.Editor
    {
        private ReorderableList _listView;
        private BonePoseLib _poseLib;

        private void OnEnable() {
            _poseLib = target as BonePoseLib;

            _listView = new ReorderableList(serializedObject, serializedObject.FindProperty("Poses"), true, true, true, true);

            _listView.headerHeight = 0;
            _listView.drawElementCallback = (rect, index, isActive, isFocused) => {
                var finalIndex = index;
                var element = _listView.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                var width = rect.width - 80;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Name"), GUIContent.none);
                rect.x += width + 10;
                width = 40;
                if (GUI.Button(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), "Apply", EditorStyles.label)) {
                    _poseLib.ApplyPose(finalIndex);
                }
                rect.x += width + 2;
                width = 30;
                if (GUI.Button(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight), "Set", EditorStyles.label)) {
                    _poseLib.SetPose(finalIndex);
                }
            };
        }

        public override void OnInspectorGUI() {
            if (!_poseLib) return;

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Poses", EditorStyles.boldLabel);
            _listView.DoLayoutList();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}