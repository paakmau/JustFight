using JustFight.Bullet;
using JustFight.Tank;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Weapon {

    class WeaponSystem : SystemBase {

        BeginInitializationEntityCommandBufferSystem m_entityCommandBufferSystem;
        protected override void OnCreate () {
            m_entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override void OnUpdate () {
            var dT = Time.DeltaTime;
            // 处理武器状态
            Dependency = Entities.ForEach ((ref WeaponState weaponState) => {
                weaponState.isShootTrigger = false;
                if (weaponState.recoveryLeftTime <= 0) {
                    weaponState.isShootTrigger = true;
                    weaponState.recoveryLeftTime = weaponState.recoveryTime;
                } else weaponState.recoveryLeftTime -= dT;
            }).ScheduleParallel (Dependency);

            // 坦克炮
            var tankGunJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var tankGunJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, in TankTurretTeam team, in AimInput shootInput, in WeaponState weaponState, in TankGun gun, in LocalToWorld localToWorld) => {
                if (weaponState.isShootTrigger) {
                    var bulletEntity = tankGunJobEcb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    tankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    var offset = localToWorld.Right * gun.offset.x + localToWorld.Up * gun.offset.y + localToWorld.Forward * gun.offset.z;
                    tankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    tankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    tankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = team.hull, id = team.id });
                }
            }).ScheduleParallel (Dependency);

            // 双管坦克炮
            var doubleTankGunJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var doubleTankGunJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, in TankTurretTeam team, in AimInput shootInput, in WeaponState weaponState, in DoubleTankGun gun, in LocalToWorld localToWorld) => {
                if (weaponState.isShootTrigger) {
                    var bulletEntity = doubleTankGunJobEcb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    var offset = localToWorld.Right * gun.offsetA.x + localToWorld.Up * gun.offsetA.y + localToWorld.Forward * gun.offsetA.z;
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = team.hull, id = team.id });
                    bulletEntity = doubleTankGunJobEcb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    offset = localToWorld.Right * gun.offsetB.x + localToWorld.Up * gun.offsetB.y + localToWorld.Forward * gun.offsetB.z;
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    doubleTankGunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = team.hull, id = team.id });
                }
            }).ScheduleParallel (Dependency);

            // 霰弹枪
            var shotgunJobEcb = m_entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
            var shotgunJobHandle = Entities.ForEach ((Entity entity, int entityInQueryIndex, in TankTurretTeam team, in AimInput shootInput, in WeaponState weaponState, in Shotgun gun, in LocalToWorld localToWorld) => {
                if (weaponState.isShootTrigger) {
                    var pos = localToWorld.Position + localToWorld.Right * gun.offset.x + localToWorld.Up * gun.offset.y + localToWorld.Forward * gun.offset.z;
                    var random = new Unity.Mathematics.Random ((uint) (dT * 10000));
                    for (int i = 0; i < gun.bulletNum; i++) {
                        var bulletEntity = shotgunJobEcb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                        var shootDir = shootInput.dir + random.NextFloat3Direction () * 0.1f;
                        shotgunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                        shotgunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = pos });
                        shotgunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * gun.bulletShootSpeed });
                        shotgunJobEcb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { hull = team.hull, id = team.id });
                    }
                }
            }).ScheduleParallel (Dependency);
            Dependency = JobHandle.CombineDependencies (tankGunJobHandle, doubleTankGunJobHandle, shotgunJobHandle);
            m_entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }
}