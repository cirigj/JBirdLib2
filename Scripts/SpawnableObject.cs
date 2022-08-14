using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JBirdLib
{

    /// <summary>
    /// Interface for passing data to a newly-spawned object.
    /// </summary>
    public interface ISpawnData {
        /// <summary>
        /// Should return a unique int id for each new object.
        /// </summary>
        int spawnID { get; }
    }

    /// <summary>
    /// Base class for spawn data. Automatically generates a new spawn ID every time, starting at 1.
    /// </summary>
    public class BaseSpawnData : ISpawnData
    {
        private static int _nextSpawnID = 1;
        public virtual int spawnID => _nextSpawnID++;
    }

    /// <summary>
    /// Base class for objects that can be spawned via a spawner system.
    /// Must be overridden with a non-generic class to use as a component.
    /// </summary>
    /// <typeparam name="SpawnData">The container type for the data to pass to a new instance of the object for initialization.</typeparam>
    public class SpawnableObject<SpawnData> : JBehaviour where SpawnData : ISpawnData
    {
        /// <summary>
        /// Automatically triggered after a new instance is made.
        /// </summary>
        protected virtual event Action<SpawnData> onSpawn;
        /// <summary>
        /// Automatically triggered before an instance is destroyed.
        /// </summary>
        protected virtual event Action onDespawn;

        public int id { get; private set; }

        /// <summary>
        /// Spawn an instanced copy of this object with its own unique ID and perform initialization on it.
        /// Should only be called on a prefab object, and will fail if it isn't.
        /// </summary>
        /// <param name="data">The spawn data to use for initialization and setting the spawn ID. Defaults to null, but will still use BaseSpawnData unique IDs.</param>
        /// <param name="position">The position to spawn the new instance at. Defaults to world origin.</param>
        /// <param name="rotation">The rotation to give the instanced object. Defaults to no rotation.</param>
        /// <param name="parent">The object to parent the new instance to. Defaults to none.</param>
        public bool Spawn (SpawnData data = default, Vector3? position = null, Quaternion? rotation = null, Transform parent = null) {
            if (gameObject.scene.name != null) {
                this.Error("Attempting to spawn non-prefab object!");
                return false;
            }
            Instantiate(this, position ?? Vector3.zero, rotation ?? Quaternion.identity, parent);
            id = data != null ? data.spawnID : new BaseSpawnData().spawnID;
            onSpawn.Invoke(data);
            return true;
        }
        
        /// <summary>
        /// Despawn an instance, calling cleanup and then destroying it.
        /// Should only be called on an instanced object, and will fail if it isn't.
        /// </summary>
        public bool Despawn () {
            if (gameObject.scene.name == null) {
                this.Error("Attempting to despawn prefab object!");
                return false;
            }
            onDespawn.Invoke();
            Destroy(gameObject);
            return true;
        }
    }

    /// <summary>
    /// Example class for spawnable objects that uses the default spawn data container type.
    /// Usable in default loot table.
    /// </summary>
    public class SpawnableObject : SpawnableObject<BaseSpawnData> {}

}
