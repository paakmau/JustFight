using System;
using Unity.Entities;

namespace JustFight {
    [Serializable]
    struct EnemySpawner : IComponentData {
        public Entity enemyPrefab;
        public float restTimePerSpawn;
        public float leftRestTime;
    }
}