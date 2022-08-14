using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBirdLib
{

    public class LootTable<LootType> : ScriptableObject
    {
        [System.Serializable]
        public class Entry {
            public LootType lootPrefab;
            public float weight;
        }

        public List<Entry> entries;


    }

    [CreateAssetMenu(fileName = "Loot Table", menuName = "JBirdLib/Loot Tables/Default")]
    public class LootTable : LootTable<SpawnableObject> {}

}
