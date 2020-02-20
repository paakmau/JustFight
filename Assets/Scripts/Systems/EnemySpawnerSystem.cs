using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace JustFight {

    class EnemySpawnerSystem : JobComponentSystem {
        [BurstCompile]
        struct EnemySpawnJob : IJobForEachWithEntity<Translation, EnemySpawner> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Translation translationCmpt, ref EnemySpawner enemySpawnCmpt) {
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