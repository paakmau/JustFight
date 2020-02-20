using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class EnemySpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject enemyPrefab = null;
        public float restTimePerSpawn = 3f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var enemySpawnCmpt = new EnemySpawner { enemyPrefab = conversionSystem.GetPrimaryEntity (enemyPrefab), restTimePerSpawn = restTimePerSpawn };
            dstManager.AddComponentData (entity, enemySpawnCmpt);
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (enemyPrefab);
        }
    }
}