using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Tank {

    class TankHullSystem : JobComponentSystem {

        [BurstCompile]
        struct DestroyTankJob : IJobForEachWithEntity<HealthPoint, HealthBarInstance, TankTurretInstance> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref HealthPoint healthCmpt, [ReadOnly] ref HealthBarInstance healthBarInstanceCmpt, [ReadOnly] ref TankTurretInstance turretInstanceCmpt) {
                if (healthCmpt.value <= 0) {
                    ecb.DestroyEntity (entityInQueryIndex, healthBarInstanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, turretInstanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, entity);
                }
            }
        }

        [BurstCompile]
        struct JumpTankJob : IJobForEach<JumpInput, JumpState, PhysicsVelocity> {
            [ReadOnly] public float dT;
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
        struct MoveTankJob : IJobForEach<MoveSpeed, MoveInput, Rotation, PhysicsVelocity> {
            [ReadOnly] public float dT;
            public void Execute ([ReadOnly] ref MoveSpeed moveSpeedCmpt, [ReadOnly] ref MoveInput moveInputCmpt, ref Rotation rotationCmpt, ref PhysicsVelocity velocityCmpt) {
                var dir = moveInputCmpt.dir;
                var curDir = math.forward (rotationCmpt.Value) + new float3 (0.00001f, 0, 0.00001f);
                var lookDirSmooth = dir;
                if (math.lengthsq (curDir - dir) > 0.001)
                    lookDirSmooth = math.lerp (curDir, dir, dT * 6);
                if (dir.x != 0 || dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (lookDirSmooth, math.up ());
                var dV = moveSpeedCmpt.value * dir * dT;
                velocityCmpt.Linear += dV;
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var destroyTankJobHandle = new DestroyTankJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var jumpTankJobHandle = new JumpTankJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var moveTankJobHandle = new MoveTankJob { dT = Time.DeltaTime }.Schedule (this, jumpTankJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyTankJobHandle);
            return JobHandle.CombineDependencies (destroyTankJobHandle, moveTankJobHandle);
        }
    }
}