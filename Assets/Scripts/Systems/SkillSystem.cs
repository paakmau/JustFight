using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class SkillSystem : JobComponentSystem {

        [BurstCompile]
        struct SkillJob : IJobForEach<SkillInput, Skill> {
            public float dT;
            public void Execute ([ReadOnly] ref SkillInput inputCmpt, ref Skill skillCmpt) {
                skillCmpt.isCastTrigger = false;
                if (skillCmpt.leftTime < 0) {
                    if (inputCmpt.isCast) {
                        skillCmpt.leftTime = skillCmpt.recoveryTime;
                        skillCmpt.isCastTrigger = true;
                    }
                } else skillCmpt.leftTime -= dT;
            }
        }

        [BurstCompile]
        struct SpraySkillJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, Skill, SpraySkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, [ReadOnly] ref Skill skillCmpt, ref SpraySkill spraySkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (spraySkillCmpt.skillLeftTime > 0) {
                    // 技能正在发动
                    spraySkillCmpt.skillShootRecoveryleftTime -= dT;
                    if (spraySkillCmpt.skillShootRecoveryleftTime < 0) {
                        spraySkillCmpt.skillShootRecoveryleftTime += spraySkillCmpt.skillShootRecoveryTime;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, spraySkillCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ()) });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + shootInputCmpt.dir * 1.7f });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInputCmpt.dir * spraySkillCmpt.skillShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                    spraySkillCmpt.skillLeftTime -= dT;
                } else if (skillCmpt.isCastTrigger) {
                    // 发动技能
                    spraySkillCmpt.skillLeftTime += spraySkillCmpt.skillLastTime;
                }
            }
        }

        [BurstCompile]
        struct BombSkillJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, Skill, BombSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public Unity.Mathematics.Random rand;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, [ReadOnly] ref Skill skillCmpt, ref BombSkill bombSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (skillCmpt.isCastTrigger) {
                    var offset = shootInputCmpt.dir * bombSkillCmpt.forwardOffset;
                    var center = localToWorldCmpt.Position + offset + new float3 (0, 6, 0);
                    for (int i = 0; i < bombSkillCmpt.bulletNum; i++) {
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, bombSkillCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        var randDir = (rand.NextFloat2Direction () * rand.NextFloat (bombSkillCmpt.radius));
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = center + new float3 (randDir.x, 0, randDir.y) });
                    }
                }

            }
        }

        [BurstCompile]
        struct ShadowSkillJob : IJobForEachWithEntity<TankTurretTeam, Skill, ShadowSkill, LocalToWorld, TankHullToFollow> {
            public float dT;
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref Skill skillCmpt, ref ShadowSkill shadowSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt, [ReadOnly] ref TankHullToFollow tankHullToFollowCmpt) {
                if (shadowSkillCmpt.skillLeftTime > 0) {
                    // 技能正在发动
                    shadowSkillCmpt.skillLeftTime -= dT;
                    if (shadowSkillCmpt.skillLeftTime <= 0) {
                        // ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceA);
                        // ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceB);
                        // ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceA);
                        // ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceB);
                    }
                } else if (skillCmpt.isCastTrigger) {
                    shadowSkillCmpt.skillLeftTime = shadowSkillCmpt.skillLastTime;
                    var offset = math.normalize (math.cross (localToWorldCmpt.Forward, math.up ())) * 3;
                    shadowSkillCmpt.shadowHullInstanceA = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowHullPrefab);
                    shadowSkillCmpt.shadowHullInstanceB = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowHullPrefab);
                    shadowSkillCmpt.shadowTurretInstanceA = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowTurretPrefab);
                    shadowSkillCmpt.shadowTurretInstanceB = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowTurretPrefab);
                    ecb.SetComponent (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = -offset });
                    ecb.SetComponent (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = -offset });
                }
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            inputDeps = new SkillJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var bombSkillJobHandle = new BombSkillJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (),
                    dT = Time.DeltaTime,
                    rand = new Unity.Mathematics.Random ((uint) (Time.DeltaTime * 10000))
            }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (bombSkillJobHandle);
            var spraySkillJobHandle = new SpraySkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (spraySkillJobHandle);
            var shadowSkillJobHandle = new ShadowSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            return JobHandle.CombineDependencies (bombSkillJobHandle, spraySkillJobHandle, shadowSkillJobHandle);
        }
    }
}