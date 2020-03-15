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

        BeginInitializationEntityCommandBufferSystem m_entityCommandBufferSystem;
        protected override void OnCreate () {
            m_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override void OnUpdate () {
            var ecb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var dT = Time.DeltaTime;
            Dependency = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BulletDestroyTime destroyTimeCmpt) => {
                destroyTimeCmpt.value -= dT;
                if (destroyTimeCmpt.value <= 0)
                    ecb.DestroyEntity (entityInQueryIndex, entity);
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }

    [UpdateBefore (typeof (StepPhysicsWorld))]
    class BulletDisableCollisionSystem : SystemBase {
        struct BulletDisableCollisionJob : IBodyPairsJob {
            [ReadOnly]
            public ComponentDataFromEntity<BulletTeam> bulletTeamFromEntity;
            public void Execute (ref ModifiableBodyPair pair) {
                bool isEntityABullet = bulletTeamFromEntity.Exists (pair.Entities.EntityA);
                bool isEntityBBullet = bulletTeamFromEntity.Exists (pair.Entities.EntityB);
                // 自己的子弹之间不会碰撞
                if (isEntityABullet && isEntityBBullet) {
                    if (bulletTeamFromEntity[pair.Entities.EntityA].hull == bulletTeamFromEntity[pair.Entities.EntityB].hull)
                        pair.Disable ();
                }
                // 自己与自己的子弹不会碰撞
                else if (isEntityABullet || isEntityBBullet) {
                    var bulletEntity = isEntityABullet ? pair.Entities.EntityA : pair.Entities.EntityB;
                    var theOtherEntity = isEntityABullet ? pair.Entities.EntityB : pair.Entities.EntityA;
                    if (bulletTeamFromEntity[pair.Entities.EntityA].hull == theOtherEntity)
                        pair.Disable ();
                }
            }
        }
        BuildPhysicsWorld m_buildPhysicsWorld;
        StepPhysicsWorld m_stepPhysicsWorld;
        protected override void OnCreate () {
            m_buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            m_stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld> ();
        }
        protected override void OnUpdate () {
            if (m_stepPhysicsWorld.Simulation.Type == SimulationType.NoPhysics) return;
            // TODO: 有GC等官方示例更新
            SimulationCallbacks.Callback callback = (ref ISimulation simulation, ref PhysicsWorld world, JobHandle inDeps) => {
                return new BulletDisableCollisionJob { bulletTeamFromEntity = GetComponentDataFromEntity<BulletTeam> () }.Schedule (simulation, ref world, inDeps);
            };
            m_stepPhysicsWorld.EnqueueCallback (SimulationCallbacks.Phase.PostCreateDispatchPairs, callback);
        }
    }

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
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
            Dependency = JobHandle.CombineDependencies (Dependency, endFramePhysicsSystem.FinalJobHandle);
            Dependency = new HitJob {
                hullTeamFromEntity = GetComponentDataFromEntity<TankHullTeam> (true),
                    bulletTeamFromEntity = GetComponentDataFromEntity<BulletTeam> (true),
                    bulletDamageFromEntity = GetComponentDataFromEntity<BulletDamage> (),
                    bulletDestroyTimeFromEntity = GetComponentDataFromEntity<BulletDestroyTime> (),
                    healthFromEntity = GetComponentDataFromEntity<HealthPoint> ()
            }.Schedule (stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, Dependency);
        }
    }
}