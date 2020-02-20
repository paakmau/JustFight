using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class SpraySkillSystem : JobComponentSystem {

        [BurstCompile]
        struct SkillJob : IJobForEachWithEntity<TankTeam, ShootInput, SkillInput, SpraySkill, GunBullet, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, [ReadOnly] ref SkillInput skillInputCmpt, ref SpraySkill skillCmpt, [ReadOnly] ref GunBullet bulletCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (skillCmpt.skillRecoveryLeftTime <= 0) {
                    if (skillCmpt.skillLeftTime > 0) {
                        // 技能正在发动
                        skillCmpt.skillShootRecoveryleftTime -= dT;
                        if (skillCmpt.skillShootRecoveryleftTime < 0) {
                            skillCmpt.skillShootRecoveryleftTime += skillCmpt.skillShootRecoveryTime;
                            var bulletEntity = ecb.Instantiate (entityInQueryIndex, bulletCmpt.bulletPrefab);
                            var forwardDir = new float3 (shootInputCmpt.dir.x, 0, shootInputCmpt.dir.y);
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (forwardDir, math.up ()) });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + forwardDir * 1f });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = forwardDir * bulletCmpt.bulletShootSpeed * 0.7f });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        }
                        skillCmpt.skillLeftTime -= dT;
                        if (skillCmpt.skillLeftTime <= 0)
                            // 技能发动结束
                            skillCmpt.skillRecoveryLeftTime += skillCmpt.skillRecoveryTime;
                    } else if (skillInputCmpt.isCast) {
                        // 发动技能
                        skillCmpt.skillLeftTime += skillCmpt.skillLastTime;
                    }
                } else skillCmpt.skillRecoveryLeftTime -= dT;
            }
        }
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var skillJobHandle = new SkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (skillJobHandle);
            return skillJobHandle;
        }
    }
}