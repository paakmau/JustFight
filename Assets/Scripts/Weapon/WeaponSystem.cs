using JustFight.Bullet;
using JustFight.Tank;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Weapon {

    class WeaponSystem : SystemBase {

        [BurstCompile]
        struct WeaponStateJob : IJobForEach<WeaponState> {
            [ReadOnly] public float dT;
            public void Execute (ref WeaponState weaponState) {
                weaponState.isShootTrigger = false;
                if (weaponState.recoveryLeftTime <= 0) {
                    weaponState.isShootTrigger = true;
                    weaponState.recoveryLeftTime = weaponState.recoveryTime;
                } else weaponState.recoveryLeftTime -= dT;
            }
        }

        [BurstCompile]
        struct TankGunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, TankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam team, [ReadOnly] ref AimInput shootInput, [ReadOnly] ref WeaponState weaponState, [ReadOnly] ref TankGun gun, [ReadOnly] ref LocalToWorld localToWorld) {
                if (weaponState.isShootTrigger) {
                    var bulletEntity = ecb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    var offset = localToWorld.Right * gun.offset.x + localToWorld.Up * gun.offset.y + localToWorld.Forward * gun.offset.z;
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = team.id });
                }
            }
        }

        [BurstCompile]
        struct DoubleTankGunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, DoubleTankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam team, [ReadOnly] ref AimInput shootInput, [ReadOnly] ref WeaponState weaponState, [ReadOnly] ref DoubleTankGun gun, [ReadOnly] ref LocalToWorld localToWorld) {
                if (weaponState.isShootTrigger) {
                    var bulletEntity = ecb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    var offset = localToWorld.Right * gun.offsetA.x + localToWorld.Up * gun.offsetA.y + localToWorld.Forward * gun.offsetA.z;
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = team.id });
                    bulletEntity = ecb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInput.dir, math.up ()) });
                    offset = localToWorld.Right * gun.offsetB.x + localToWorld.Up * gun.offsetB.y + localToWorld.Forward * gun.offsetB.z;
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorld.Position + offset });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInput.dir * gun.bulletShootSpeed });
                    ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = team.id });
                }
            }
        }

        [BurstCompile]
        struct ShotgunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, Shotgun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam team, [ReadOnly] ref AimInput shootInput, [ReadOnly] ref WeaponState weaponState, [ReadOnly] ref Shotgun gun, [ReadOnly] ref LocalToWorld localToWorld) {
                if (weaponState.isShootTrigger) {
                    var pos = localToWorld.Position + localToWorld.Right * gun.offset.x + localToWorld.Up * gun.offset.y + localToWorld.Forward * gun.offset.z;
                    var random = new Unity.Mathematics.Random ((uint) (dT * 10000));
                    for (int i = 0; i < gun.bulletNum; i++) {
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, gun.bulletPrefab);
                        var shootDir = shootInput.dir + random.NextFloat3Direction () * 0.1f;
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = pos });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * gun.bulletShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = team.id });
                    }
                }
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override void OnUpdate () {
            Dependency = new WeaponStateJob { dT = Time.DeltaTime }.Schedule (this, Dependency);
            var tankGunJobHandle = new TankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, Dependency);
            var doubleTankGunJobHandle = new DoubleTankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, Dependency);
            var shotgunJobHandle = new ShotgunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, Dependency);
            Dependency = JobHandle.CombineDependencies (tankGunJobHandle, doubleTankGunJobHandle, shotgunJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (Dependency);
        }
    }
}