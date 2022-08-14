using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBirdLib
{
    // TODO: implementation, utilizing object pooling as a potential option
    public class Spawner<SpawnType,SpawnData> : JBehaviour where SpawnType : SpawnableObject<SpawnData> where SpawnData : ISpawnData {
        
    }

    public class Spawner : Spawner<SpawnableObject,BaseSpawnData> { }

}