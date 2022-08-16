using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace JBirdLib
{

    [Flags]
    public enum LootChanceType {
        Guaranteed = 0,
        Weighted = 1 << 0,
        RequireTagsAll = 1 << 1,
        RequireTagsAny = 1 << 2,
    }

    [Serializable]
    public class LootChance {

        public LootChance() {
            chanceType = LootChanceType.Weighted;
            minRoll = 1;
            maxRoll = 1;
            numPerRoll = 1;
            boundsAsRange = true;
            extraRollChance = 0.5f;
            allowMultiples = true;
            weight = 1;
            skipIfPartialTags = true;
            skipIfNoTags = true;
        }

        public LootChanceType chanceType;
        // all types
        [Min(0)]
        public int minRoll;
        [Min(0)]
        public int maxRoll;
        [Min(1)]
        public int numPerRoll;
        public bool boundsAsRange; // toggles extraRollChance and rollIndependently
        [Range(0f,1f)]
        public float extraRollChance;
        public bool rollIndependently;
        // weighted
        [Min(0f)]
        public float weight;
        public bool allowMultiples;
        // require all tags
        [SerializeField]
        private string allTagList = "";
        public List<string> allTags {
            get {
                return allTagList.Trim().Split(',').ToList();
            }
            set {
                allTagList = value.Aggregate("", (s, t) => s == "" ? t : s + "," + t);
            }
        }
        public bool skipIfPartialTags; // only visible if weighted
        // require any tags
        [SerializeField]
        private string anyTagList = "";
        public List<string> anyTags {
            get {
                return anyTagList.Trim().Split(',').ToList();
            }
            set {
                anyTagList = value.Aggregate("", (s, t) => s == "" ? t : s + "," + t);
            }
        }
        public bool skipIfNoTags; // only visible if weighted

        public int Roll(System.Random random) {
            int total;
            if (boundsAsRange) {
                total = random.Next(minRoll, maxRoll + 1);
            }
            else {
                total = minRoll * numPerRoll;
                int potentialExtraRolls = maxRoll - minRoll;
                for (int i = 0; i < potentialExtraRolls; i++) {
                    if ((float)random.NextDouble() < extraRollChance) {
                        total += numPerRoll;
                    }
                    else if (!rollIndependently) {
                        break;
                    }
                }
            }
            return total;
        }
    }

    public class LootTable<LootType> : ScriptableObject
    {
        [System.Serializable]
        public class Entry {
            public LootType lootPrefab;
            public LootChance chance;
        }

        public class LootRoll {
            public LootRoll(LootType prefab, int roll) {
                lootPrefab = prefab;
                amount = roll;
            }

            public LootType lootPrefab;
            public int amount;
        }

        public List<Entry> entries;

        private static IEnumerable<Entry> FilterForTags (IEnumerable<Entry> entries, string[] tags) {
            if (tags.Count() == 0) {
                return entries;
            }
            return entries
                .Where(e => {
                    if (e.chance.chanceType.Contains(LootChanceType.RequireTagsAll)) {
                        return e.chance.allTags.All(t => tags.Contains(t));
                    }
                    else {
                        return true;
                    }
                })
                .Where(e => {
                    if (e.chance.chanceType.Contains(LootChanceType.RequireTagsAny)) {
                        return e.chance.anyTags.All(t => tags.Contains(t));
                    }
                    else {
                        return true;
                    }
                });
        }

        bool ShouldIncludeWeight (Entry entry, string[] tags) {
            bool r = true;
            if (entry.chance.chanceType.Contains(LootChanceType.RequireTagsAll) && entry.chance.skipIfPartialTags) {
                r &= entry.chance.allTags.All(t => tags.Contains(t));
            }
            if (entry.chance.chanceType.Contains(LootChanceType.RequireTagsAny) && entry.chance.skipIfNoTags) {
                r &= entry.chance.anyTags.Any(t => tags.Contains(t));
            }
            return r;
        }

        bool TagsMatch (Entry entry, string[] tags) {
            bool r = true;
            if (entry.chance.chanceType.Contains(LootChanceType.RequireTagsAll)) {
                r &= entry.chance.allTags.All(t => tags.Contains(t));
            }
            if (entry.chance.chanceType.Contains(LootChanceType.RequireTagsAny)) {
                r &= entry.chance.anyTags.Any(t => tags.Contains(t));
            }
            return r;
        }

        public List<LootRoll> GetLoot(int weightedRolls = 1, int? seed = null, params string[] tags) {
            var random = seed != null ? new System.Random(seed.Value) : new System.Random();
            var loot = new List<LootRoll>();
            // non-weighted entries
            var nonWeightedEntries = entries.Where(e => !e.chance.chanceType.Contains(LootChanceType.Weighted));
            var entriesToRoll = FilterForTags(nonWeightedEntries, tags);
            foreach (var entry in entriesToRoll) {
                loot.Add(new LootRoll(entry.lootPrefab, entry.chance.Roll(random)));
            }
            // weighted entries
            var weightedEntries = entries.Where(e => e.chance.chanceType.Contains(LootChanceType.Weighted)).ToList();
            for (int i = 0; i < weightedRolls; i++) {
                if (weightedEntries.Count == 0) {
                    break;
                }
                float maxWeight = weightedEntries.Sum(e => ShouldIncludeWeight(e, tags) ? e.chance.weight : 0f);
                float randomValue = (float)random.NextDouble() * maxWeight;
                Entry selectedEntry = null;
                foreach (Entry entry in weightedEntries) {
                    if (randomValue < entry.chance.weight) {
                        selectedEntry = entry;
                        if (TagsMatch(entry, tags)) {
                            loot.Add(new LootRoll(entry.lootPrefab, entry.chance.Roll(random)));
                        }
                        break;
                    }
                    randomValue -= entry.chance.weight;
                }
                if (!selectedEntry.chance.allowMultiples) {
                    weightedEntries.Remove(selectedEntry);
                }
            }
            return loot;
        }
    }

    [CreateAssetMenu(fileName = "Loot Table", menuName = "JBirdLib/Loot Tables/Default")]
    public class LootTable : LootTable<string> { }

}
