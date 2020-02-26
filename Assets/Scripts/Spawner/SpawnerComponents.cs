using System;
using Unity.Entities;

namespace JustFight.Spawner {

    [Serializable]
    struct SelfSpawner : IComponentData {
        public Entity hullPrefab;
        public Entity turretPrefab;
        public Entity healthBarPrefab;
        public int teamId;
    }

    [Serializable]
    struct EnemySpawner : IComponentData {
        public Entity hullPrefab;
        public Entity turretPrefab;
        public Entity healthBarPrefab;
        public int enemyNum;
        public float restTimePerSpawn;
        public float leftRestTime;
        public int teamId;
    }
}