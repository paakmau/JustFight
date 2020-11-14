using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Spawner {

    class EnemySpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject hullPrefab = null;
        public GameObject turretPrefab = null;
        public GameObject healthBarPrefab = null;
        public int enemyNum = 4;
        public float restTimePerSpawn = 3f;
        public int teamId = 1;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var enemySpawnCmpt = new EnemySpawner {
                hullPrefab = conversionSystem.GetPrimaryEntity (hullPrefab),
                turretPrefab = conversionSystem.GetPrimaryEntity (turretPrefab),
                healthBarPrefab = conversionSystem.GetPrimaryEntity (healthBarPrefab),
                enemyNum = enemyNum,
                restTimePerSpawn = restTimePerSpawn,
                teamId = teamId
            };
            dstManager.AddComponentData (entity, enemySpawnCmpt);
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (hullPrefab);
            referencedPrefabs.Add (turretPrefab);
            referencedPrefabs.Add (healthBarPrefab);
        }
    }
}