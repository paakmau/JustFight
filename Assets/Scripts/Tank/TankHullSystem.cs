using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Tank {

    class TankHullSystem : SystemBase {

        BeginInitializationEntityCommandBufferSystem m_entityCommandBufferSystem;
        protected override void OnCreate () {
            m_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override void OnUpdate () {
            // 销毁坦克
            var ecb = m_entityCommandBufferSystem.CreateCommandBuffer ().AsParallelWriter ();
            var destroyTankJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, in HealthPoint healthCmpt, in HealthBarInstance healthBarInstanceCmpt, in TankTurretInstance turretInstanceCmpt) => {
                if (healthCmpt.value <= 0) {
                    ecb.DestroyEntity (entityInQueryIndex, healthBarInstanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, turretInstanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, entity);
                }
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (destroyTankJobHandle);

            // 坦克跳跃
            var dT = Time.DeltaTime;
            var jumpTankJobHandle = Entities.ForEach ((ref JumpState jumpStateCmpt, ref PhysicsVelocity velocityCmpt, in JumpInput jumpInputCmpt) => {
                if (jumpStateCmpt.leftRecoveryTime > 0) {
                    jumpStateCmpt.leftRecoveryTime -= dT;
                } else {
                    if (jumpInputCmpt.isJump) {
                        velocityCmpt.Linear.y += jumpStateCmpt.speed;
                        jumpStateCmpt.leftRecoveryTime = jumpStateCmpt.recoveryTime;
                    }
                }
            }).ScheduleParallel (Dependency);

            // 坦克移动
            var moveTankJobHandle = Entities.ForEach ((ref Rotation rotationCmpt, ref PhysicsVelocity velocityCmpt, in MoveSpeed moveSpeedCmpt, in MoveInput moveInputCmpt) => {
                var dir = moveInputCmpt.dir;
                if (dir.x != 0 || dir.z != 0)
                    rotationCmpt.Value = math.slerp (rotationCmpt.Value, quaternion.LookRotation (dir, math.up ()), dT * 5);
                var dV = moveSpeedCmpt.value * dir * dT;
                velocityCmpt.Linear += dV;
            }).ScheduleParallel (jumpTankJobHandle);

            Dependency = JobHandle.CombineDependencies (destroyTankJobHandle, moveTankJobHandle);
        }
    }
}