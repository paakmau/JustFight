using System;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Tank.Spawner {
    class SelfSpawner : IComponentData {
        public Entity hullPrefab;
        public Entity turretPrefab;
        public int teamId;
        public Transform followCameraTransform;
    }

    [Serializable]
    struct EnemySpawner : IComponentData {
        public Entity hullPrefab;
        public Entity turretPrefab;
        public int enemyNum;
        public float restTimePerSpawn;
        public float leftRestTime;
        public int teamId;
    }
}