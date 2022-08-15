using UnityEditor;
using UnityEngine;
using System;

namespace JBirdLib
{

    [CustomPropertyDrawer(typeof(LootChance))]
    public class LootChanceDrawer : PropertyDrawer
    {
        float padding = 0.1f;
        float headerPadding = 0.3f;
        float maxHeight = 1f;
        float tagErrorPadding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float fields = 5f + headerPadding;
            if (!property.FindPropertyRelative("boundsAsRange").boolValue) {
                fields += 4f + headerPadding;
            }
            int weightFlag = Convert.ToInt32(LootChanceType.Weighted);
            bool isWeighted = (property.FindPropertyRelative("chanceType").intValue & weightFlag) == weightFlag;
            if (isWeighted) {
                fields += 3f + headerPadding;
            }
            int allTagsFlag = Convert.ToInt32(LootChanceType.RequireTagsAll);
            bool requireAllTags = (property.FindPropertyRelative("chanceType").intValue & allTagsFlag) == allTagsFlag;
            if (requireAllTags) {
                fields += isWeighted ? 2f : 1f;
            }
            int anyTagsFlag = Convert.ToInt32(LootChanceType.RequireTagsAny);
            bool requireAnyTags = (property.FindPropertyRelative("chanceType").intValue & anyTagsFlag) == anyTagsFlag;
            if (requireAnyTags) {
                fields += isWeighted ? 2f : 1f;
            }
            if (requireAllTags || requireAnyTags) {
                fields += 1f + headerPadding + tagErrorPadding;
            }
            maxHeight = fields + padding * (fields - 1f);
            return base.GetPropertyHeight(property, label) * maxHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth *= 0.75f;

            float x = position.x;
            float y = position.y;
            float w = position.width;
            float h = position.height / maxHeight;

            EditorGUI.BeginChangeCheck();

            // Chance type

            property.FindPropertyRelative("chanceType").intValue =
                Convert.ToInt32(EditorGUI.EnumFlagsField(
                    new Rect(x, y, w, h),
                    "Type",
                    (LootChanceType)Enum.ToObject(typeof(LootChanceType), property.FindPropertyRelative("chanceType").intValue)
                ));
            y += h * (1f + padding);

            // Persistant options

            y += h * headerPadding / 2f;
            EditorGUI.DrawRect(
                new Rect(x, y, w, 1f),
                Color.gray
            );
            y += h * headerPadding / 2f;
            EditorGUI.LabelField(
                new Rect(x, y, w, h),
                "    Standard Options:    ",
                EditorStyles.boldLabel
            );
            y += h * (1f + padding);

            bool boundsAsRange = property.FindPropertyRelative("boundsAsRange").boolValue;
            property.FindPropertyRelative("minRoll").intValue =
                EditorGUI.IntField(
                    new Rect(x, y, w, h),
                    boundsAsRange ? "Min Drops" : "Min Rolls",
                    property.FindPropertyRelative("minRoll").intValue
                );
            y += h * (1f + padding);

            property.FindPropertyRelative("maxRoll").intValue =
                EditorGUI.IntField(
                    new Rect(x, y, w, h),
                    boundsAsRange ? "Max Drops" : "Max Rolls",
                    property.FindPropertyRelative("maxRoll").intValue
                );
            y += h * (1f + padding);

            property.FindPropertyRelative("boundsAsRange").boolValue =
                EditorGUI.ToggleLeft(
                    new Rect(x, y, w, h),
                    "Use As Flat Range",
                    property.FindPropertyRelative("boundsAsRange").boolValue
                );
            y += h * (1f + padding);

            // Extra roll options

            boundsAsRange = property.FindPropertyRelative("boundsAsRange").boolValue;
            if (!boundsAsRange) {

                y += h * headerPadding / 2f;
                EditorGUI.DrawRect(
                    new Rect(x, y, w, 1f),
                    Color.gray
                );
                y += h * headerPadding / 2f;
                EditorGUI.LabelField(
                    new Rect(x, y, w, h),
                    "    Additional Rolls:    ",
                    EditorStyles.boldLabel
                );
                y += h * (1f + padding);

                property.FindPropertyRelative("extraRollChance").floatValue =
                    EditorGUI.Slider(
                        new Rect(x, y, w, h),
                        "Extra Roll %",
                        property.FindPropertyRelative("extraRollChance").floatValue,
                        0f,
                        1f
                    );
                y += h * (1f + padding);

                property.FindPropertyRelative("numPerRoll").intValue =
                    EditorGUI.IntField(
                        new Rect(x, y, w, h),
                        "Drops Per Roll",
                        property.FindPropertyRelative("numPerRoll").intValue
                    );
                y += h * (1f + padding);

                property.FindPropertyRelative("rollIndependently").boolValue =
                    EditorGUI.ToggleLeft(
                        new Rect(x, y, w, h),
                        "Roll Independently",
                        property.FindPropertyRelative("rollIndependently").boolValue
                    );
                y += h * (1f + padding);
            }

            int weightFlag = Convert.ToInt32(LootChanceType.Weighted);
            bool isWeighted = (property.FindPropertyRelative("chanceType").intValue & weightFlag) == weightFlag;
            int allTagsFlag = Convert.ToInt32(LootChanceType.RequireTagsAll);
            bool requireAllTags = (property.FindPropertyRelative("chanceType").intValue & allTagsFlag) == allTagsFlag;
            int anyTagsFlag = Convert.ToInt32(LootChanceType.RequireTagsAny);
            bool requireAnyTags = (property.FindPropertyRelative("chanceType").intValue & anyTagsFlag) == anyTagsFlag;

            // Weighting options

            if (isWeighted) {

                y += h * headerPadding / 2f;
                EditorGUI.DrawRect(
                    new Rect(x, y, w, 1f),
                    Color.gray
                );
                y += h * headerPadding / 2f;
                EditorGUI.LabelField(
                    new Rect(x, y, w, h),
                    "    Weighted Chance:    ",
                    EditorStyles.boldLabel
                );
                y += h * (1f + padding);

                property.FindPropertyRelative("weight").floatValue =
                    EditorGUI.FloatField(
                        new Rect(x, y, w, h),
                        "Weight",
                        property.FindPropertyRelative("weight").floatValue
                    );
                y += h * (1f + padding);

                property.FindPropertyRelative("allowMultiples").boolValue =
                    !EditorGUI.ToggleLeft(
                        new Rect(x, y, w, h),
                        "Skip If Already Rolled",
                        !property.FindPropertyRelative("allowMultiples").boolValue
                    );
                y += h * (1f + padding);

                if (requireAllTags) {
                    property.FindPropertyRelative("skipIfPartialTags").boolValue =
                        EditorGUI.ToggleLeft(
                            new Rect(x, y, w, h),
                            "Skip If \"All\" Tags Missing",
                            property.FindPropertyRelative("skipIfPartialTags").boolValue
                        );
                    y += h * (1f + padding);
                }

                if (requireAnyTags) {
                    property.FindPropertyRelative("skipIfNoTags").boolValue =
                        EditorGUI.ToggleLeft(
                            new Rect(x, y, w, h),
                            "Skip If \"Any\" Tags Missing",
                            property.FindPropertyRelative("skipIfNoTags").boolValue
                        );
                    y += h * (1f + padding);
                }
            }

            // Require Tags options

            if (requireAllTags || requireAnyTags) {

                y += h * headerPadding / 2f;
                EditorGUI.DrawRect(
                    new Rect(x, y, w, 1f),
                    Color.gray
                );
                y += h * headerPadding / 2f;
                EditorGUI.LabelField(
                    new Rect(x, y, w, h),
                    "    Required Tags:    ",
                    EditorStyles.boldLabel
                );
                y += h * (1f + padding);

                EditorGUI.LabelField(
                    new Rect(x, y, w, h * tagErrorPadding),
                    "Tags should be a comma-seperated list of strings with no spaces.",
                    EditorStyles.helpBox
                );
                y += h * (tagErrorPadding + padding);
            }

            if (requireAllTags) {
                property.FindPropertyRelative("allTagList").stringValue =
                    EditorGUI.TextField(
                        new Rect(x, y, w, h),
                        "Require All Of:",
                        property.FindPropertyRelative("allTagList").stringValue
                    );
                y += h * (1f + padding);
                
            }

            if (requireAnyTags) {
                property.FindPropertyRelative("anyTagList").stringValue =
                    EditorGUI.TextField(
                        new Rect(x, y, w, h),
                        "Require Any Of:",
                        property.FindPropertyRelative("anyTagList").stringValue
                    );
                y += h * (1f + padding);

            }

            EditorGUI.EndChangeCheck();

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth *= 2f;

            EditorGUI.EndProperty();
        }
    }

}
