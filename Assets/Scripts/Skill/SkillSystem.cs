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
    class SkillSystem : SystemBase {

        [BurstCompile]
        struct ShadowSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, ShadowSkill, LocalToWorld, TankHullToFollow> {
            [ReadOnly] public float dT;
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInput, [ReadOnly] ref Skill skill, ref ShadowSkill shadowskill, [ReadOnly] ref LocalToWorld localToWorld, [ReadOnly] ref TankHullToFollow tankHullToFollowCmpt) {
                if (skill.lastLeftTime > 0)
                    shadowskill.aimDir = aimInput.dir;
                if (skill.isCastTrigger) {
                    var offset = math.normalize (math.cross (localToWorld.Forward, math.up ())) * 3;
                    var hullInstanceA = ecb.Instantiate (entityInQueryIndex, shadowskill.shadowHullPrefab);
                    var hullInstanceB = ecb.Instantiate (entityInQueryIndex, shadowskill.shadowHullPrefab);
                    var turretInstanceA = ecb.Instantiate (entityInQueryIndex, shadowskill.shadowTurretPrefab);
                    var turretInstanceB = ecb.Instantiate (entityInQueryIndex, shadowskill.shadowTurretPrefab);
                    ecb.SetComponent (entityInQueryIndex, hullInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, hullInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = -offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = -offset });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceA, new ShadowTurret { turretEntity = entity });
                    ecb.SetComponent (entityInQueryIndex, turretInstanceB, new ShadowTurret { turretEntity = entity });
                    var newShadowskill = shadowskill;
                    newShadowskill.shadowHullInstanceA = hullInstanceA;
                    newShadowskill.shadowHullInstanceB = hullInstanceB;
                    newShadowskill.shadowTurretInstanceA = turretInstanceA;
                    newShadowskill.shadowTurretInstanceB = turretInstanceB;
                    ecb.SetComponent (entityInQueryIndex, entity, newShadowskill);
                }
                if (skill.isEndTrigger) {
                    ecb.DestroyEntity (entityInQueryIndex, shadowskill.shadowTurretInstanceA);
                    ecb.DestroyEntity (entityInQueryIndex, shadowskill.shadowTurretInstanceB);
                    ecb.DestroyEntity (entityInQueryIndex, shadowskill.shadowHullInstanceA);
                    ecb.DestroyEntity (entityInQueryIndex, shadowskill.shadowHullInstanceB);
                }
            }
        }

        [BurstCompile]
        struct ShotgunBurstSkillJob : IJobForEachWithEntity<TankTurretTeam, AimInput, Skill, ShotgunBurstSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput aimInput, [ReadOnly] ref Skill skill, ref ShotgunBurstSkill burstskill, [ReadOnly] ref LocalToWorld localToWorld) {
                if (skill.lastLeftTime > 0) {
                    // 技能正在发动
                    burstskill.skillShootRecoveryleftTime -= dT;
                    if (burstskill.skillShootRecoveryleftTime < 0) {
                        burstskill.skillShootRecoveryleftTime += burstskill.skillShootRecoveryTime;
                        var random = new Unity.Mathematics.Random ((uint) (dT * 10000));
                        float3 dir = aimInput.dir;
                        if (burstskill.upRot != 0)
                            dir = math.rotate (quaternion.AxisAngle (math.cross (dir, math.up ()), burstskill.upRot), dir);
                        var offset = localToWorld.Position + localToWorld.Right * burstskill.offset.x + localToWorld.Up * burstskill.offset.y + localToWorld.Forward * burstskill.offset.z;
                        for (int i = 0; i < burstskill.bulletNumPerShoot; i++) {
                            var shootDir = dir + random.NextFloat3Direction () * burstskill.spread;
                            var bulletEntity = ecb.Instantiate (entityInQueryIndex, burstskill.bulletPrefab);
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = offset });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * burstskill.skillShootSpeed });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        }
                    }
                }
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override void OnUpdate () {
            // 处理技能冷却与持续
            var dT = Time.DeltaTime;
            Dependency = Entities.ForEach ((ref Skill skill, ref WeaponState weaponState, in AimInput input) => {
                skill.isCastTrigger = false;
                skill.isEndTrigger = false;
                if (skill.recoveryLeftTime <= 0) {
                    if (input.isCast) {
                        skill.recoveryLeftTime = skill.recoveryTime;
                        skill.isCastTrigger = true;
                        skill.lastLeftTime = skill.lastTime;
                    }
                } else {
                    skill.recoveryLeftTime -= dT;
                    if (skill.lastLeftTime > 0) {
                        // 禁用武器
                        if (skill.isDisableWeapon)
                            weaponState.recoveryLeftTime = weaponState.recoveryTime;
                        skill.lastLeftTime -= dT;
                        if (skill.lastLeftTime <= 0)
                            skill.isEndTrigger = true;
                    }
                }
            }).ScheduleParallel (Dependency);

            // 处理速射技能
            var burstSkillJobEcb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var burstSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BurstSkill burstskill, in LocalToWorld localToWorld, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill) => {
                if (skill.lastLeftTime > 0) {
                    // 技能正在发动
                    burstskill.skillShootRecoveryleftTime -= dT;
                    if (burstskill.skillShootRecoveryleftTime < 0) {
                        burstskill.skillShootRecoveryleftTime += burstskill.skillShootRecoveryTime;
                        var offset = localToWorld.Right * burstskill.offset.x + localToWorld.Up * burstskill.offset.y + localToWorld.Forward * burstskill.offset.z;
                        var bulletEntity = burstSkillJobEcb.Instantiate (entityInQueryIndex, burstskill.bulletPrefab);
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (aimInput.dir, math.up ()) });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = aimInput.dir * burstskill.skillShootSpeed });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                }
            }).ScheduleParallel (Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (burstSkillJobHandle);

            // 处理轰炸技能
            var bombSkillJobEcb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var rand = new Unity.Mathematics.Random ((uint) (dT * 10000));
            var bombSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BombSkill bombskill, in LocalToWorld localToWorld, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill)=> {
                if (skill.isCastTrigger) {
                    var offset = aimInput.dir * bombskill.forwardOffset;
                    var center = localToWorld.Position + offset + new float3 (0, 15, 0);
                    for (int i = 0; i < bombskill.bulletNum; i++) {
                        var bulletEntity = bombSkillJobEcb.Instantiate (entityInQueryIndex, bombskill.bulletPrefab);
                        bombSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        var randDir = (rand.NextFloat2Direction () * rand.NextFloat (bombskill.radius));
                        bombSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = center + new float3 (randDir.x, 0, randDir.y) });
                    }
                }
            }).ScheduleParallel(Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (bombSkillJobHandle);


            var shadowSkillJobHandle = new ShadowSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (shadowSkillJobHandle);
            var shotgunBurstSkillJobHandle = new ShotgunBurstSkillJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, Dependency);
            entityCommandBufferSystem.AddJobHandleForProducer (shotgunBurstSkillJobHandle);
            Dependency = JobHandle.CombineDependencies (JobHandle.CombineDependencies (bombSkillJobHandle, burstSkillJobHandle, shadowSkillJobHandle), shotgunBurstSkillJobHandle);
        }
    }
}