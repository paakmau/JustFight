using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace JustFight {

    [Serializable]
    struct BulletTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct BulletDamage : IComponentData {
        public int value;
    }

    [Serializable]
    struct BulletDestroyTime : IComponentData {
        public float value;
    }

    [RequiresEntityConversion]
    class BulletBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        public int damage = 15;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<BulletTeam> (entity);
            dstManager.AddComponentData (entity, new BulletDamage { value = damage });
            dstManager.AddComponentData (entity, new BulletDestroyTime { value = 10 });
        }
    }

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
    // [UpdateBefore (typeof (EndFramePhysicsSystem))]
    class BulletSystem : JobComponentSystem {
        [BurstCompile]
        struct HitJob : ICollisionEventsJob {
            public ComponentDataFromEntity<TankTeam> tankTeamGroup;
            public ComponentDataFromEntity<BulletTeam> bulletTeamGroup;
            public ComponentDataFromEntity<BulletDamage> bulletDamageGroup;
            public ComponentDataFromEntity<BulletDestroyTime> bulletDestroyTimeGroup;
            public ComponentDataFromEntity<Health> healthGroup;
            void DisableBullet (Entity bulletEntity) {
                bulletDamageGroup[bulletEntity] = new BulletDamage { value = 0 };
                var destroyTimeCmpt = bulletDestroyTimeGroup[bulletEntity];
                destroyTimeCmpt.value = math.min (destroyTimeCmpt.value, 0.6f);
                bulletDestroyTimeGroup[bulletEntity] = destroyTimeCmpt;
            }
            public void Execute (CollisionEvent collisionEvent) {
                // TODO: shit
                var entityA = collisionEvent.Entities.EntityA;
                var entityB = collisionEvent.Entities.EntityB;
                bool isEntityABullet = bulletDamageGroup.Exists (entityA);
                bool isEntityBBullet = bulletDamageGroup.Exists (entityB);
                var bulletEntity = isEntityABullet ? entityA : entityB;
                var tankEntity = isEntityABullet ? entityB : entityA;
                var bulletBodyId = isEntityABullet ? collisionEvent.BodyIndices.BodyAIndex : collisionEvent.BodyIndices.BodyBIndex;

                if (healthGroup.Exists (tankEntity)) {
                    var bulletTeamCmpt = bulletTeamGroup[bulletEntity];
                    var tankTeamCmpt = tankTeamGroup[tankEntity];
                    if (bulletTeamCmpt.id != tankTeamCmpt.id) {
                        var dmgCmpt = bulletDamageGroup[bulletEntity];
                        var healthCmpt = healthGroup[tankEntity];
                        healthCmpt.value -= dmgCmpt.value;
                        healthGroup[tankEntity] = healthCmpt;
                    }
                }
                if (isEntityABullet) DisableBullet (entityA);
                if (isEntityBBullet) DisableBullet (entityB);
            }
        }

        [BurstCompile]
        struct DestroyJob : IJobForEachWithEntity<BulletDestroyTime> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, ref BulletDestroyTime destroyTimeCmpt) {
                destroyTimeCmpt.value -= dT;
                if (destroyTimeCmpt.value <= 0)
                    ecb.DestroyEntity (entityInQueryIndex, entity);
            }
        }
        BuildPhysicsWorld buildPhysicsWorldSystem;
        StepPhysicsWorld stepPhysicsWorldSystem;
        EndFramePhysicsSystem endFramePhysicsSystem;
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld> ();
            endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem> ();
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var hitJobHandle = new HitJob {
                tankTeamGroup = GetComponentDataFromEntity<TankTeam> (), bulletTeamGroup = GetComponentDataFromEntity<BulletTeam> (), bulletDamageGroup = GetComponentDataFromEntity<BulletDamage> (), bulletDestroyTimeGroup = GetComponentDataFromEntity<BulletDestroyTime> (), healthGroup = GetComponentDataFromEntity<Health> ()
            }.Schedule (stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDeps);
            endFramePhysicsSystem.HandlesToWaitFor.Add (hitJobHandle);
            var destroyJobHandle = new DestroyJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime
            }.Schedule (this, hitJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyJobHandle);
            return destroyJobHandle;
        }
    }
}