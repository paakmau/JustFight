using JustFight.Tank;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace JustFight.Bullet {

    class BulletLiftTimeSystem : SystemBase {

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

        protected override void OnUpdate () {
            Dependency = new DestroyJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime
            }.Schedule (this, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }

    [UpdateAfter (typeof (StepPhysicsWorld)), UpdateBefore (typeof (EndFramePhysicsSystem))]
    class BulletHitSystem : SystemBase {
        [BurstCompile]
        struct HitJob : ICollisionEventsJob {
            [ReadOnly]
            public ComponentDataFromEntity<TankHullTeam> hullTeamFromEntity;
            [ReadOnly]
            public ComponentDataFromEntity<BulletTeam> bulletTeamFromEntity;
            public ComponentDataFromEntity<BulletDamage> bulletDamageFromEntity;
            public ComponentDataFromEntity<BulletDestroyTime> bulletDestroyTimeFromEntity;
            public ComponentDataFromEntity<HealthPoint> healthFromEntity;
            void DisableBullet (Entity bulletEntity) {
                bulletDamageFromEntity[bulletEntity] = new BulletDamage { value = 0 };
                var destroyTimeCmpt = bulletDestroyTimeFromEntity[bulletEntity];
                destroyTimeCmpt.value = math.min (destroyTimeCmpt.value, 0.3f);
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
        EndFramePhysicsSystem endFramePhysicsSystem;
        protected override void OnCreate () {
            buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld> ();
            endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem> ();
        }
        protected override void OnUpdate () {
            Dependency = new HitJob {
                hullTeamFromEntity = GetComponentDataFromEntity<TankHullTeam> (true),
                    bulletTeamFromEntity = GetComponentDataFromEntity<BulletTeam> (true),
                    bulletDamageFromEntity = GetComponentDataFromEntity<BulletDamage> (),
                    bulletDestroyTimeFromEntity = GetComponentDataFromEntity<BulletDestroyTime> (),
                    healthFromEntity = GetComponentDataFromEntity<HealthPoint> ()
            }.Schedule (stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, Dependency);
            endFramePhysicsSystem.HandlesToWaitFor.Add (Dependency);
        }
    }
}