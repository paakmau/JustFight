using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {
    [Serializable]
    struct EnemySpawn : IComponentData {
        public Entity enemyPrefab;
        public float restTimePerSpawn;
        public float leftRestTime;
    }

    [RequiresEntityConversion]
    class EnemySpawnerBehaviour : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject enemyPrefab = null;
        public float restTimePerSpawn = 3f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var enemySpawnCmpt = new EnemySpawn { enemyPrefab = conversionSystem.GetPrimaryEntity (enemyPrefab), restTimePerSpawn = restTimePerSpawn };
            dstManager.AddComponentData (entity, enemySpawnCmpt);
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (enemyPrefab);
        }
    }

    class EnemySpawnerSystem : JobComponentSystem {
        [BurstCompile]
        struct EnemySpawnJob : IJobForEachWithEntity<Translation, EnemySpawn> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Translation translationCmpt, ref EnemySpawn enemySpawnCmpt) {
                enemySpawnCmpt.leftRestTime -= dT;
                if (enemySpawnCmpt.leftRestTime <= 0) {
                    enemySpawnCmpt.leftRestTime += enemySpawnCmpt.restTimePerSpawn;
                    var enemyEntity = ecb.Instantiate (entityInQueryIndex, enemySpawnCmpt.enemyPrefab);
                    ecb.SetComponent (entityInQueryIndex, enemyEntity, translationCmpt);
                }
            }
        }
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var jobHandle = new EnemySpawnJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (jobHandle);
            return jobHandle;
        }
    }
}