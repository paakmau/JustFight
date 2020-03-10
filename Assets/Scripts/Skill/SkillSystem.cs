using JustFight.Bullet;
using JustFight.Tank;
using JustFight.Weapon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Skill {

    [UpdateBefore (typeof (WeaponSystem))]
    class SkillSystem : JobComponentSystem {

        [BurstCompile]
        struct SkillJob : IJobForEach<AimInput, Skill> {
            [ReadOnly] public float dT;
            public void Execute ([ReadOnly] ref AimInput inputCmpt, ref Skill skillCmpt) {
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
        struct BurstSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, WeaponState, BurstSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInputCmpt, [ReadOnly] ref Skill skillCmpt, ref WeaponState weaponStateCmpt, ref BurstSkill burstSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (burstSkillCmpt.skillLeftTime > 0) {
                    // 技能正在发动
                    burstSkillCmpt.skillShootRecoveryleftTime -= dT;
                    if (burstSkillCmpt.skillShootRecoveryleftTime < 0) {
                        burstSkillCmpt.skillShootRecoveryleftTime += burstSkillCmpt.skillShootRecoveryTime;
                        var offset = localToWorldCmpt.Right * burstSkillCmpt.offset.x + localToWorldCmpt.Up * burstSkillCmpt.offset.y + localToWorldCmpt.Forward * burstSkillCmpt.offset.z;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, burstSkillCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (aimInputCmpt.dir, math.up ()) });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + offset });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = aimInputCmpt.dir * burstSkillCmpt.skillShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                    burstSkillCmpt.skillLeftTime -= dT;
                    // 禁止武器
                    weaponStateCmpt.recoveryLeftTime = weaponStateCmpt.recoveryTime;
                } else if (skillCmpt.isCastTrigger) {
                    // 发动技能
                    burstSkillCmpt.skillLeftTime += burstSkillCmpt.skillLastTime;
                }
            }
        }

        [BurstCompile]
        struct BombSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, BombSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public Unity.Mathematics.Random rand;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInputCmpt, [ReadOnly] ref Skill skillCmpt, ref BombSkill bombSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (skillCmpt.isCastTrigger) {
                    var offset = aimInputCmpt.dir * bombSkillCmpt.forwardOffset;
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
        struct ShadowSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, ShadowSkill, LocalToWorld, TankHullToFollow> {
            [ReadOnly] public float dT;
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInputCmpt, [ReadOnly] ref Skill skillCmpt, ref ShadowSkill shadowSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt, [ReadOnly] ref TankHullToFollow tankHullToFollowCmpt) {
                if (shadowSkillCmpt.skillLeftTime > 0) {
                    // 技能正在发动
                    shadowSkillCmpt.skillLeftTime -= dT;
                    shadowSkillCmpt.aimDir = aimInputCmpt.dir;
                    if (shadowSkillCmpt.skillLeftTime <= 0) {
                        ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceA);
                        ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowTurretInstanceB);
                        ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceA);
                        ecb.DestroyEntity (entityInQueryIndex, shadowSkillCmpt.shadowHullInstanceB);
                    }
                } else if (skillCmpt.isCastTrigger) {
                    shadowSkillCmpt.skillLeftTime = shadowSkillCmpt.skillLastTime;
                    var offset = math.normalize (math.cross (localToWorldCmpt.Forward, math.up ())) * 3;
                    var hullInstanceA = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowHullPrefab);
                    var hullInstanceB = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowHullPrefab);
                    var turretInstanceA = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowTurretPrefab);
                    var turretInstanceB = ecb.Instantiate (entityInQueryIndex, shadowSkillCmpt.shadowTurretPrefab);
                    ecb.SetComponent (entityInQueryIndex, hullInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, hullInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = -offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = -offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceA, new ShadowTurret { turretEntity = entity });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceB, new ShadowTurret { turretEntity = entity });
                    var newShadowSkillCmpt = shadowSkillCmpt;
                    newShadowSkillCmpt.shadowHullInstanceA = hullInstanceA;
                    newShadowSkillCmpt.shadowHullInstanceB = hullInstanceB;
                    newShadowSkillCmpt.shadowTurretInstanceA = turretInstanceA;
                    newShadowSkillCmpt.shadowTurretInstanceB = turretInstanceB;
                    ecb.SetComponent (entityInQueryIndex, entity, newShadowSkillCmpt);
                }
            }
        }

        [BurstCompile]
        struct ShotgunBurstSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, WeaponState, ShotgunBurstSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInputCmpt, [ReadOnly] ref Skill skillCmpt, ref WeaponState weaponStateCmpt, ref ShotgunBurstSkill burstSkillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (burstSkillCmpt.skillLeftTime > 0) {
                    // 技能正在发动
                    burstSkillCmpt.skillShootRecoveryleftTime -= dT;
                    if (burstSkillCmpt.skillShootRecoveryleftTime < 0) {
                        burstSkillCmpt.skillShootRecoveryleftTime += burstSkillCmpt.skillShootRecoveryTime;
                        var random = new Unity.Mathematics.Random ((uint) (dT * 10000));
                        float3 dir = aimInputCmpt.dir;
                        if (burstSkillCmpt.upRot != 0)
                            dir = math.rotate (quaternion.AxisAngle (math.cross (dir, math.up ()), burstSkillCmpt.upRot), dir);
                        var offset = localToWorldCmpt.Position + localToWorldCmpt.Right * burstSkillCmpt.offset.x + localToWorldCmpt.Up * burstSkillCmpt.offset.y + localToWorldCmpt.Forward * burstSkillCmpt.offset.z;
                        for (int i = 0; i < burstSkillCmpt.bulletNumPerShoot; i++) {
                            var shootDir = dir + random.NextFloat3Direction () * burstSkillCmpt.spread;
                            var bulletEntity = ecb.Instantiate (entityInQueryIndex, burstSkillCmpt.bulletPrefab);
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = offset });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * burstSkillCmpt.skillShootSpeed });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        }
                    }
                    burstSkillCmpt.skillLeftTime -= dT;
                    // 禁用武器
                    weaponStateCmpt.recoveryLeftTime = weaponStateCmpt.recoveryTime;
                } else if (skillCmpt.isCastTrigger) {
                    // 发动技能
                    burstSkillCmpt.skillLeftTime += burstSkillCmpt.skillLastTime;
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
            var burstSkillJobHandle = new BurstSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (burstSkillJobHandle);
            var shadowSkillJobHandle = new ShadowSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (shadowSkillJobHandle);
            var shotgunBurstSkillJobHandle = new ShotgunBurstSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, burstSkillJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (shotgunBurstSkillJobHandle);
            return JobHandle.CombineDependencies (JobHandle.CombineDependencies (bombSkillJobHandle, burstSkillJobHandle, shadowSkillJobHandle), shotgunBurstSkillJobHandle);
        }
    }
}