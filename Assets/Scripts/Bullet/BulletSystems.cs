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
            var ecb = m_entityCommandBufferSystem.CreateCommandBuffer ().AsParallelWriter ();
            var dT = Time.DeltaTime;
            Dependency = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BulletDestroyTime destroyTimeCmpt) => {
                destroyTimeCmpt.value -= dT;
                if (destroyTimeCmpt.value <= 0)
                    ecb.DestroyEntity (entityInQueryIndex, entity);
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore (typeof (StepPhysicsWorld))]
    class BulletDisableCollisionSystem : SystemBase {
        struct BulletDisableCollisionJob : IBodyPairsJob {
            [ReadOnly]
            public ComponentDataFromEntity<BulletTeam> bulletTeamFromEntity;
            public void Execute (ref ModifiableBodyPair pair) {
                bool isEntityABullet = bulletTeamFromEntity.HasComponent (pair.EntityA);
                bool isEntityBBullet = bulletTeamFromEntity.HasComponent (pair.EntityB);
                // 自己的子弹之间不会碰撞
                if (isEntityABullet && isEntityBBullet) {
                    if (bulletTeamFromEntity[pair.EntityA].hull == bulletTeamFromEntity[pair.EntityB].hull)
                        pair.Disable ();
                }
                // 自己与自己的子弹不会碰撞
                else if (isEntityABullet || isEntityBBullet) {
                    var bulletEntity = isEntityABullet ? pair.EntityA : pair.EntityB;
                    var theOtherEntity = isEntityABullet ? pair.EntityB : pair.EntityA;
                    if (bulletTeamFromEntity[pair.EntityA].hull == theOtherEntity)
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
                inDeps.Complete();
                return new BulletDisableCollisionJob { bulletTeamFromEntity = GetComponentDataFromEntity<BulletTeam> () }.Schedule (simulation, ref world, inDeps);
            };
            m_stepPhysicsWorld.EnqueueCallback (SimulationCallbacks.Phase.PostCreateDispatchPairs, callback);
        }
    }

    [UpdateInGroup (typeof (FixedStepSimulationSystemGroup))]
    [UpdateAfter (typeof (ExportPhysicsWorld))]
    [UpdateBefore (typeof (EndFramePhysicsSystem))]
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
                var entityA = collisionEvent.EntityA;
                var entityB = collisionEvent.EntityB;
                bool isEntityABullet = bulletDamageFromEntity.HasComponent (entityA);
                bool isEntityBBullet = bulletDamageFromEntity.HasComponent (entityB);
                var bulletEntity = isEntityABullet ? entityA : entityB;
                var hullEntity = isEntityABullet ? entityB : entityA;
                var bulletBodyId = isEntityABullet ? collisionEvent.BodyIndexA : collisionEvent.BodyIndexB;

                if (healthFromEntity.HasComponent (hullEntity) && bulletTeamFromEntity[bulletEntity].id != hullTeamFromEntity[hullEntity].id) {
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
            endFramePhysicsSystem.AddInputDependency (Dependency);
        }
    }
}