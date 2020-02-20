using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class TankHullSystem : JobComponentSystem {

        [BurstCompile]
        struct InstantiateHealthBarAndTankTurretJob : IJobForEachWithEntity<HealthBarPrefab, TankTurretPrefab, Translation> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref HealthBarPrefab healthBarPrefabCmpt, [ReadOnly] ref TankTurretPrefab turretPrefabCmpt, [ReadOnly] ref Translation translationCmpt) {
                var healthBarEntity = ecb.Instantiate (entityInQueryIndex, healthBarPrefabCmpt.entity);
                ecb.SetComponent (entityInQueryIndex, healthBarEntity, new TankToFollow { entity = entity, offset = new float3 (-1.8f, 0, 0) });
                ecb.RemoveComponent<HealthBarPrefab> (entityInQueryIndex, entity);
                ecb.AddComponent (entityInQueryIndex, entity, new HealthBarInstance { entity = healthBarEntity });

                var turretEntity = ecb.Instantiate (entityInQueryIndex, turretPrefabCmpt.entity);
                ecb.SetComponent (entityInQueryIndex, turretEntity, new TankToFollow { entity = entity });
                ecb.RemoveComponent<TankTurretPrefab> (entityInQueryIndex, entity);
                ecb.AddComponent (entityInQueryIndex, entity, new TankTurretInstance { entity = turretEntity });
            }
        }

        [BurstCompile]
        struct DestroyTankJob : IJobForEachWithEntity<Health, HealthBarInstance> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Health healthCmpt, [ReadOnly] ref HealthBarInstance instanceCmpt) {
                if (healthCmpt.value <= 0) {
                    ecb.DestroyEntity (entityInQueryIndex, instanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, entity);
                }
            }
        }

        [BurstCompile]
        struct JumpTankJob : IJobForEach<JumpInput, JumpState, PhysicsVelocity> {
            public float dT;
            public void Execute ([ReadOnly] ref JumpInput jumpInputCmpt, ref JumpState jumpStateCmpt, ref PhysicsVelocity velocityCmpt) {
                if (jumpStateCmpt.leftRecoveryTime > 0) {
                    jumpStateCmpt.leftRecoveryTime -= dT;
                } else {
                    if (jumpInputCmpt.isJump) {
                        velocityCmpt.Linear.y += jumpStateCmpt.speed;
                        jumpStateCmpt.leftRecoveryTime = jumpStateCmpt.recoveryTime;
                    }
                }
            }
        }

        [BurstCompile]
        struct MoveTankJob : IJobForEach<MoveInput, Rotation, PhysicsVelocity> {
            public void Execute ([ReadOnly] ref MoveInput moveInputCmpt, ref Rotation rotationCmpt, ref PhysicsVelocity velocityCmpt) {
                var dir = new float3 (moveInputCmpt.dir.x, 0, moveInputCmpt.dir.y);
                if (dir.x != 0 || dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (dir, math.up ());
                dir *= 4;
                dir.y = velocityCmpt.Linear.y;
                velocityCmpt.Linear = dir;
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var instantiateJobHandle = new InstantiateHealthBarAndTankTurretJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var destroyTankJobHandle = new DestroyTankJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var jumpTankJobHandle = new JumpTankJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var moveTankJobHandle = new MoveTankJob ().Schedule (this, jumpTankJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (instantiateJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyTankJobHandle);
            return JobHandle.CombineDependencies (instantiateJobHandle, destroyTankJobHandle, moveTankJobHandle);
        }
    }
}