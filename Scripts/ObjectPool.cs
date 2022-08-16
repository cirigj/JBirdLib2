using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBirdLib
{
    // TODO: Make object pool
    public class ObjectPool<PrefabType> : JBehaviour where PrefabType : JBehaviour
    {
        public PrefabType prefab;

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }

    public class ObjectPool : ObjectPool<SpawnableObject> { }

}
