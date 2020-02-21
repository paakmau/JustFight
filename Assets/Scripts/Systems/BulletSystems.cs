using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace JustFight {

    [UpdateBefore (typeof (BuildPhysicsWorld))]
    class BulletLiftTimeSystem : JobComponentSystem {
        // TODO: 需要解决Collider Filter初始化

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

        [BurstCompile]
        struct ModifyFilterJob : IJobForEachWithEntity<BulletTeam, PhysicsCollider> {
            public EntityCommandBuffer.Concurrent ecb;
            public unsafe void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref BulletTeam teamCmpt, ref PhysicsCollider colliderCmpt) {
                var filter = colliderCmpt.Value.Value.Filter;
                filter.GroupIndex = -teamCmpt.id;
                colliderCmpt.Value.Value.Filter = filter;
                ecb.RemoveComponent<BulletTeam> (entityInQueryIndex, entity);
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var destroyJobHandle = new DestroyJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime
            }.Schedule (this, inputDeps);
            var modifyFilterJobHandle = new ModifyFilterJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ()
            }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (modifyFilterJobHandle);
            return JobHandle.CombineDependencies (destroyJobHandle, modifyFilterJobHandle);
        }
    }

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
    class BulletHitSystem : JobComponentSystem {
        [BurstCompile]
        struct HitJob : ICollisionEventsJob {
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
                    var dmgCmpt = bulletDamageGroup[bulletEntity];
                    var healthCmpt = healthGroup[tankEntity];
                    healthCmpt.value -= dmgCmpt.value;
                    healthGroup[tankEntity] = healthCmpt;
                }
                if (isEntityABullet) DisableBullet (entityA);
                if (isEntityBBullet) DisableBullet (entityB);
            }
        }
        BuildPhysicsWorld buildPhysicsWorldSystem;
        StepPhysicsWorld stepPhysicsWorldSystem;
        protected override void OnCreate () {
            buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var hitJobHandle = new HitJob {
                bulletDamageGroup = GetComponentDataFromEntity<BulletDamage> (), bulletDestroyTimeGroup = GetComponentDataFromEntity<BulletDestroyTime> (), healthGroup = GetComponentDataFromEntity<Health> ()
            }.Schedule (stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDeps);
            return hitJobHandle;
        }
    }
}