using UnityEngine;
using UnityEditor;

namespace JBirdLib
{
    [CustomPropertyDrawer(typeof(Angle))]
    public class AngleDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth *= 0.5f;

            Rect fRect = new Rect(position.x, position.y, position.width, position.height);
            property.FindPropertyRelative("_val").floatValue =
                EditorGUI.Slider(
                    fRect,
                    new GUIContent("\u03B8"), 
                    property.FindPropertyRelative("_val").floatValue,
                    0f,
                    360f
                );

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth *= 2f;

            EditorGUI.EndProperty();
        }
    }

}
