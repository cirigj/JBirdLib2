using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JBirdLib
{
    [CustomEditor(typeof(LootTable))]
    public class LootTableEditor : Editor
    {
        int numRolls = 1;
        int seed = (int)DateTime.Now.Ticks;
        List<string> tags = new List<string>();
        List<LootTable.LootRoll> roll = new List<LootTable.LootRoll>();

        public string FormatRoll(List<LootTable.LootRoll> roll) {
            return roll.Aggregate("", (s, i) => s + (s == "" ? "" : "\n") + string.Format(" - {0} x{1}", i.lootPrefab.ToString(), i.amount));
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            LootTable table = target as LootTable;

            numRolls = EditorGUILayout.IntField("Number of Weighted Items", numRolls);
            seed = EditorGUILayout.IntField("Random Seed", seed);
            if (GUILayout.Button("Randomize Seed")) {
                seed = (int)DateTime.Now.Ticks;
            }
            tags = EditorGUILayout.TextField("Tags", tags.Aggregate("", (s, t) => s == "" ? t : s + "," + t)).Trim().Split(',').ToList();

            if (GUILayout.Button("Seeded Test Roll!")) {
                roll = table.GetLoot(numRolls, seed, tags.ToArray());
            }

            if (GUILayout.Button("Random Test Roll!")) {
                roll = table.GetLoot(numRolls, null, tags.ToArray());
            }

            if (roll.Count > 0) {
                GUI.enabled = false;
                EditorGUILayout.TextArea(FormatRoll(roll), EditorStyles.textArea);
                GUI.enabled = true;
            }
        }
    }
}
