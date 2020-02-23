using JustFight.Health;
using JustFight.TankHull;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace JustFight.Bullet {

    class BulletLiftTimeSystem : JobComponentSystem {

        [BurstCompile]
        struct DestroyJob : IJobForEachWithEntity<BulletDestroyTime> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, ref BulletDestroyTime destroyTimeCmpt) {
                destroyTimeCmpt.value -= dT;
                if (destroyTimeCmpt.value <= 0)
                    ecb.DestroyEntity (entityInQueryIndex, entity);
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
            entityCommandBufferSystem.AddJobHandleForProducer (destroyJobHandle);
            return destroyJobHandle;
        }
    }

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
    class BulletHitSystem : JobComponentSystem {
        [BurstCompile]
        struct HitJob : ICollisionEventsJob {
            public ComponentDataFromEntity<TankHullTeam> hullTeamFromEntity;
            public ComponentDataFromEntity<BulletTeam> bulletTeamFromEntity;
            public ComponentDataFromEntity<BulletDamage> bulletDamageFromEntity;
            public ComponentDataFromEntity<BulletDestroyTime> bulletDestroyTimeFromEntity;
            public ComponentDataFromEntity<HealthPoint> healthFromEntity;
            void DisableBullet (Entity bulletEntity) {
                bulletDamageFromEntity[bulletEntity] = new BulletDamage { value = 0 };
                var destroyTimeCmpt = bulletDestroyTimeFromEntity[bulletEntity];
                destroyTimeCmpt.value = math.min (destroyTimeCmpt.value, 0.6f);
                bulletDestroyTimeFromEntity[bulletEntity] = destroyTimeCmpt;
            }
            public void Execute (CollisionEvent collisionEvent) {
                // TODO: shit
                var entityA = collisionEvent.Entities.EntityA;
                var entityB = collisionEvent.Entities.EntityB;
                bool isEntityABullet = bulletDamageFromEntity.Exists (entityA);
                bool isEntityBBullet = bulletDamageFromEntity.Exists (entityB);
                var bulletEntity = isEntityABullet ? entityA : entityB;
                var hullEntity = isEntityABullet ? entityB : entityA;
                var bulletBodyId = isEntityABullet ? collisionEvent.BodyIndices.BodyAIndex : collisionEvent.BodyIndices.BodyBIndex;

                if (healthFromEntity.Exists (hullEntity) && bulletTeamFromEntity[bulletEntity].id != hullTeamFromEntity[hullEntity].id) {
                    var dmgCmpt = bulletDamageFromEntity[bulletEntity];
                    var healthCmpt = healthFromEntity[hullEntity];
                    healthCmpt.value -= dmgCmpt.value;
                    healthFromEntity[hullEntity] = healthCmpt;
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
                hullTeamFromEntity = GetComponentDataFromEntity<TankHullTeam> (),
                    bulletTeamFromEntity = GetComponentDataFromEntity<BulletTeam> (),
                    bulletDamageFromEntity = GetComponentDataFromEntity<BulletDamage> (),
                    bulletDestroyTimeFromEntity = GetComponentDataFromEntity<BulletDestroyTime> (),
                    healthFromEntity = GetComponentDataFromEntity<HealthPoint> ()
            }.Schedule (stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, inputDeps);
            return hitJobHandle;
        }
    }
}