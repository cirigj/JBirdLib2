using UnityEditor;
using UnityEngine;

namespace JBirdLib
{
    
    [CustomPropertyDrawer(typeof(AngleVector3))]
    public class AngleVector3Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth *= 0.5f;

            float w = position.width;
            float h = position.height / 4.3f;
            var aRect = new Rect(position.x, position.y, w, h);
            var eRect = new Rect(position.x, position.y + 1.1f * h, w, h);
            var mRect = new Rect(position.x, position.y + 2.2f * h, w, h);
            var vRect = new Rect(position.x, position.y + 3.3f * h, w, h);

            EditorGUI.BeginChangeCheck();

            EditorGUI.Slider(aRect, property.FindPropertyRelative("_azimuth"), 0, 360);
            EditorGUI.Slider(eRect, property.FindPropertyRelative("_elevation"), 0, 360);
            EditorGUI.PropertyField(mRect, property.FindPropertyRelative("magnitude"));

            float a = property.FindPropertyRelative("_azimuth").floatValue;
            float e = property.FindPropertyRelative("_elevation").floatValue;
            float m = property.FindPropertyRelative("magnitude").floatValue;
            Vector3 v = VectorHelper.FromAzimuthAndElevation(a, e) * m;

            GUI.enabled = false;
            EditorGUI.Vector3Field(vRect, GUIContent.none, v);
            GUI.enabled = true;

            EditorGUI.EndChangeCheck();

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth *= 2f;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 4.3f;
        }
    }

}
