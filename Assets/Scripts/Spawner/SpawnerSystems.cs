using JustFight.Input;
using JustFight.Tank;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight.Spawner {
    class EnemySpawnerSystem : SystemBase {

        [BurstCompile]
        struct EnemySpawnerJob : IJobForEachWithEntity<Translation, Rotation, EnemySpawner> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Translation translationCmpt, [ReadOnly] ref Rotation rotationCmpt, ref EnemySpawner spawnerCmpt) {
                spawnerCmpt.leftRestTime -= dT;
                if (spawnerCmpt.enemyNum > 0) {
                    if (spawnerCmpt.leftRestTime <= 0) {
                        spawnerCmpt.enemyNum--;
                        spawnerCmpt.leftRestTime += spawnerCmpt.restTimePerSpawn;
                        var hullEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.hullPrefab);
                        ecb.SetComponent (entityInQueryIndex, hullEntity, translationCmpt);
                        ecb.SetComponent (entityInQueryIndex, hullEntity, rotationCmpt);
                        ecb.SetComponent (entityInQueryIndex, hullEntity, new TankHullTeam { id = spawnerCmpt.teamId });
                        ecb.AddComponent (entityInQueryIndex, hullEntity, new EnemyHull { random = new Unity.Mathematics.Random ((uint) (dT * 10000)) });

                        var turretEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.turretPrefab);
                        ecb.SetComponent (entityInQueryIndex, turretEntity, rotationCmpt);
                        ecb.SetComponent (entityInQueryIndex, turretEntity, new TankHullToFollow { entity = hullEntity });
                        ecb.SetComponent (entityInQueryIndex, turretEntity, new TankTurretTeam { id = spawnerCmpt.teamId });
                        ecb.AddComponent (entityInQueryIndex, turretEntity, new EnemyTurret { random = new Unity.Mathematics.Random ((uint) (dT * 10000)) });

                        var healthBarEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.healthBarPrefab);
                        ecb.SetComponent (entityInQueryIndex, healthBarEntity, new TankHullToFollow { entity = hullEntity, offset = new float3 (-1.8f, 0, 0) });

                        ecb.SetComponent (entityInQueryIndex, hullEntity, new TankTurretInstance { entity = turretEntity });
                        ecb.SetComponent (entityInQueryIndex, hullEntity, new HealthBarInstance { entity = healthBarEntity });
                    }
                } else ecb.DestroyEntity (entityInQueryIndex, entity);
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override void OnUpdate () {
            Dependency = new EnemySpawnerJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }

    class SelfSpawnerSystem : SystemBase {

        [BurstCompile]
        struct SelfSpawnerJob : IJobForEachWithEntity<Translation, Rotation, SelfSpawner> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Translation translationCmpt, [ReadOnly] ref Rotation rotationCmpt, ref SelfSpawner spawnerCmpt) {

                var hullEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.hullPrefab);
                ecb.SetComponent (entityInQueryIndex, hullEntity, translationCmpt);
                ecb.SetComponent (entityInQueryIndex, hullEntity, rotationCmpt);
                ecb.SetComponent (entityInQueryIndex, hullEntity, new TankHullTeam { id = spawnerCmpt.teamId });
                ecb.AddComponent (entityInQueryIndex, hullEntity, new SelfHull { });

                var turretEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.turretPrefab);
                ecb.SetComponent (entityInQueryIndex, turretEntity, rotationCmpt);
                ecb.SetComponent (entityInQueryIndex, turretEntity, new TankHullToFollow { entity = hullEntity });
                ecb.SetComponent (entityInQueryIndex, turretEntity, new TankTurretTeam { id = spawnerCmpt.teamId });
                ecb.AddComponent (entityInQueryIndex, turretEntity, new SelfTurret { });

                var healthBarEntity = ecb.Instantiate (entityInQueryIndex, spawnerCmpt.healthBarPrefab);
                ecb.SetComponent (entityInQueryIndex, healthBarEntity, new TankHullToFollow { entity = hullEntity, offset = new float3 (-1.8f, 0, 0) });

                ecb.SetComponent (entityInQueryIndex, hullEntity, new TankTurretInstance { entity = turretEntity });
                ecb.SetComponent (entityInQueryIndex, hullEntity, new HealthBarInstance { entity = healthBarEntity });

                ecb.DestroyEntity (entityInQueryIndex, entity);
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override void OnUpdate () {
            Dependency = new SelfSpawnerJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }
}