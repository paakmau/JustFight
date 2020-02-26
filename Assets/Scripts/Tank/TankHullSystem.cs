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
            public void Execute ([ReadOnly] ref MoveSpeed moveSpeedCmpt, [ReadOnly] ref MoveInput moveInputCmpt, ref Rotation rotationCmpt, ref PhysicsVelocity velocityCmpt) {
                var dir = moveInputCmpt.dir;
                if (dir.x != 0 || dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (dir, math.up ());
                var v = moveSpeedCmpt.value * dir;
                v.y = velocityCmpt.Linear.y;
                velocityCmpt.Linear = v;
                // TODO: 需要参考UnityPhysicsSample中的CharacterController
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var destroyTankJobHandle = new DestroyTankJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var jumpTankJobHandle = new JumpTankJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var moveTankJobHandle = new MoveTankJob ().Schedule (this, jumpTankJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyTankJobHandle);
            return JobHandle.CombineDependencies (destroyTankJobHandle, moveTankJobHandle);
        }
    }
}