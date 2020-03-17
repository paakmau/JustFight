using JustFight.Bullet;
using JustFight.Tank;
using JustFight.Weapon;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Skill {

    [UpdateBefore (typeof (WeaponSystem))]
    class SkillSystem : SystemBase {

        Random m_random;

        BeginInitializationEntityCommandBufferSystem m_entityCommandBufferSystem;

        protected override void OnCreate () {
            m_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
            m_random = new Random (19990922);
        }

        protected override void OnUpdate () {
            // 保证随机均匀
            m_random.NextUInt ();
            var random = m_random;

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
            var burstSkillJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var burstSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BurstSkill burstskill, in LocalToWorld localToWorld, in TankHullToFollow hullToFollow, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill) => {
                if (skill.lastLeftTime > 0) {
                    // 技能正在发动
                    burstskill.skillShootRecoveryleftTime -= dT;
                    if (burstskill.skillShootRecoveryleftTime < 0) {
                        burstskill.skillShootRecoveryleftTime += burstskill.skillShootRecoveryTime;
                        var offset = localToWorld.Right * burstskill.offsetX;
                        var bulletEntity = burstSkillJobEcb.Instantiate (entityInQueryIndex, burstskill.bulletPrefab);
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (aimInput.dir, math.up ()) });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = aimInput.dir * burstskill.skillShootSpeed });
                        burstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = hullToFollow.entity, id = teamCmpt.id });
                    }
                }
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (burstSkillJobHandle);

            // 处理轰炸技能
            var bombSkillJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var bombSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref BombSkill bombskill, in LocalToWorld localToWorld, in TankHullToFollow hullToFollow, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill) => {
                var rand = random;
                if (skill.isCastTrigger) {
                    var offset = aimInput.dir * bombskill.forwardOffset;
                    var center = localToWorld.Position + offset + new float3 (0, 15, 0);
                    for (int i = 0; i < bombskill.bulletNum; i++) {
                        var bulletEntity = bombSkillJobEcb.Instantiate (entityInQueryIndex, bombskill.bulletPrefab);
                        bombSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = hullToFollow.entity, id = teamCmpt.id });
                        var randDir = (rand.NextFloat2Direction () * rand.NextFloat (bombskill.radius));
                        bombSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = center + new float3 (randDir.x, 0, randDir.y) });
                    }
                }
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (bombSkillJobHandle);

            // 处理影分身技能
            var shadowSkillJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var shadowSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref ShadowSkill shadowskill, in LocalToWorld localToWorld, in TankHullToFollow tankHullToFollowCmpt, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill) => {
                if (skill.lastLeftTime > 0)
                    shadowskill.aimDir = aimInput.dir;
                if (skill.isCastTrigger) {
                    var offset = math.normalize (math.cross (localToWorld.Forward, math.up ())) * 3;
                    var hullInstanceA = shadowSkillJobEcb.Instantiate (entityInQueryIndex, shadowskill.shadowHullPrefab);
                    var hullInstanceB = shadowSkillJobEcb.Instantiate (entityInQueryIndex, shadowskill.shadowHullPrefab);
                    var turretInstanceA = shadowSkillJobEcb.Instantiate (entityInQueryIndex, shadowskill.shadowTurretPrefab);
                    var turretInstanceB = shadowSkillJobEcb.Instantiate (entityInQueryIndex, shadowskill.shadowTurretPrefab);
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, hullInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = offset });
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, hullInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = tankHullToFollowCmpt.entity, offset = -offset });
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, turretInstanceA, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = offset });
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, turretInstanceB, new Shadow { translationEntity = tankHullToFollowCmpt.entity, rotationEntity = entity, offset = -offset });
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, turretInstanceA, new ShadowTurret { turretEntity = entity });
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, turretInstanceB, new ShadowTurret { turretEntity = entity });
                    var newShadowskill = shadowskill;
                    newShadowskill.shadowHullInstanceA = hullInstanceA;
                    newShadowskill.shadowHullInstanceB = hullInstanceB;
                    newShadowskill.shadowTurretInstanceA = turretInstanceA;
                    newShadowskill.shadowTurretInstanceB = turretInstanceB;
                    shadowSkillJobEcb.SetComponent (entityInQueryIndex, entity, newShadowskill);
                }
                if (skill.isEndTrigger) {
                    shadowSkillJobEcb.DestroyEntity (entityInQueryIndex, shadowskill.shadowTurretInstanceA);
                    shadowSkillJobEcb.DestroyEntity (entityInQueryIndex, shadowskill.shadowTurretInstanceB);
                    shadowSkillJobEcb.DestroyEntity (entityInQueryIndex, shadowskill.shadowHullInstanceA);
                    shadowSkillJobEcb.DestroyEntity (entityInQueryIndex, shadowskill.shadowHullInstanceB);
                }
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (shadowSkillJobHandle);

            // 处理霰弹枪技能
            var shotgunBurstSkillJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var shotgunBurstSkillJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, ref ShotgunBurstSkill burstskill, in TankHullToFollow hullToFollow, in TankTurretTeam teamCmpt, in AimInput aimInput, in Skill skill, in LocalToWorld localToWorld) => {
                if (skill.lastLeftTime > 0) {
                    // 技能正在发动
                    burstskill.skillShootRecoveryleftTime -= dT;
                    if (burstskill.skillShootRecoveryleftTime < 0) {
                        burstskill.skillShootRecoveryleftTime += burstskill.skillShootRecoveryTime;
                        var rand = random;
                        float3 dir = aimInput.dir;
                        if (burstskill.upRot != 0)
                            dir = math.rotate (quaternion.AxisAngle (math.cross (dir, math.up ()), burstskill.upRot), dir);
                        for (int i = 0; i < burstskill.bulletNumPerShoot; i++) {
                            float3 randDir;
                            if (burstskill.isFlat) {
                                float2 temp = rand.NextFloat2Direction ();
                                randDir = new float3 (temp.x, 0, temp.y);
                            } else randDir = rand.NextFloat3Direction ();
                            var shootDir = dir + randDir * burstskill.spread;
                            var bulletEntity = shotgunBurstSkillJobEcb.Instantiate (entityInQueryIndex, burstskill.bulletPrefab);
                            shotgunBurstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                            shotgunBurstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position });
                            shotgunBurstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * burstskill.skillShootSpeed });
                            shotgunBurstSkillJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = hullToFollow.entity, id = teamCmpt.id });
                        }
                    }
                }
            }).ScheduleParallel (Dependency);
            m_entityCommandBufferSystem.AddJobHandleForProducer (shotgunBurstSkillJobHandle);

            Dependency = JobHandle.CombineDependencies (JobHandle.CombineDependencies (bombSkillJobHandle, burstSkillJobHandle, shadowSkillJobHandle), shotgunBurstSkillJobHandle);
        }
    }
}