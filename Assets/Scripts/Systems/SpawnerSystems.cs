using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace JustFight {
    // TODO: Add self recover feature and combine the two system
    class EnemySpawnerSystem : JobComponentSystem {
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
                        ecb.AddComponent (entityInQueryIndex, turretEntity, new EnemyTurret { random = new Unity.Mathematics.Random ((uint) (dT * 1000)) });

                        ecb.AddComponent (entityInQueryIndex, hullEntity, new TankTurretInstance { entity = turretEntity });
                    }
                } else ecb.DestroyEntity (entityInQueryIndex, entity);
            }
        }
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var jobHandle = new EnemySpawnerJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (jobHandle);
            return jobHandle;
        }
    }

    class SelfSpawnerSystem : ComponentSystem {

        protected override void OnUpdate () {
            Entities.WithAllReadOnly (typeof (Translation), typeof (Rotation)).ForEach ((Entity entity, SelfSpawner spawnerCmpt, ref Translation translationCmpt, ref Rotation rotationCmpt) => {
                var hullEntity = EntityManager.Instantiate (spawnerCmpt.hullPrefab);
                EntityManager.SetComponentData (hullEntity, translationCmpt);
                EntityManager.SetComponentData (hullEntity, rotationCmpt);
                EntityManager.SetComponentData (hullEntity, new TankHullTeam { id = spawnerCmpt.teamId });
                EntityManager.AddComponentData (hullEntity, new SelfHull ());
                EntityManager.AddComponentObject (hullEntity, new FollowCamera { transform = spawnerCmpt.followCameraTransform });

                var turretEntity = EntityManager.Instantiate (spawnerCmpt.turretPrefab);
                EntityManager.SetComponentData (turretEntity, rotationCmpt);
                EntityManager.SetComponentData (turretEntity, new TankHullToFollow { entity = hullEntity });
                EntityManager.SetComponentData (turretEntity, new TankTurretTeam { id = spawnerCmpt.teamId });
                EntityManager.AddComponentData (turretEntity, new SelfTurret ());

                EntityManager.SetComponentData (hullEntity, new TankTurretInstance { entity = turretEntity });

                EntityManager.DestroyEntity (entity);
            });
        }
    }
}