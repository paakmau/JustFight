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

    class WeaponSystem : JobComponentSystem {

        [BurstCompile]
        struct TankGunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, TankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput shootInputCmpt, ref WeaponState weaponStateCmpt, [ReadOnly] ref TankGun gunCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (weaponStateCmpt.recoveryLeftTime <= 0) {
                    if (shootInputCmpt.isShoot) {
                        weaponStateCmpt.recoveryLeftTime += weaponStateCmpt.recoveryTime;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, gunCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ()) });
                        var offset = localToWorldCmpt.Right * gunCmpt.offset.x + localToWorldCmpt.Up * gunCmpt.offset.y + localToWorldCmpt.Forward * gunCmpt.offset.z;
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + offset });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInputCmpt.dir * gunCmpt.bulletShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                } else weaponStateCmpt.recoveryLeftTime -= dT;
            }
        }

        [BurstCompile]
        struct DoubleTankGunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, DoubleTankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput shootInputCmpt, ref WeaponState weaponStateCmpt, [ReadOnly] ref DoubleTankGun gunCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (weaponStateCmpt.recoveryLeftTime <= 0) {
                    if (shootInputCmpt.isShoot) {
                        weaponStateCmpt.recoveryLeftTime += weaponStateCmpt.recoveryTime;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, gunCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ()) });
                        var offset = localToWorldCmpt.Right * gunCmpt.offsetA.x + localToWorldCmpt.Up * gunCmpt.offsetA.y + localToWorldCmpt.Forward * gunCmpt.offsetA.z;
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + offset });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInputCmpt.dir * gunCmpt.bulletShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        bulletEntity = ecb.Instantiate (entityInQueryIndex, gunCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ()) });
                        offset = localToWorldCmpt.Right * gunCmpt.offsetB.x + localToWorldCmpt.Up * gunCmpt.offsetB.y + localToWorldCmpt.Forward * gunCmpt.offsetB.z;
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + offset });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInputCmpt.dir * gunCmpt.bulletShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                } else weaponStateCmpt.recoveryLeftTime -= dT;
            }
        }

        [BurstCompile]
        struct ShotgunJob : IJobForEachWithEntity<TankTurretTeam, AimInput, WeaponState, Shotgun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref AimInput shootInputCmpt, ref WeaponState weaponStateCmpt, [ReadOnly] ref Shotgun gunCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (weaponStateCmpt.recoveryLeftTime <= 0) {
                    if (shootInputCmpt.isShoot) {
                        weaponStateCmpt.recoveryLeftTime += weaponStateCmpt.recoveryTime;
                        var offset = localToWorldCmpt.Right * gunCmpt.offset.x + localToWorldCmpt.Up * gunCmpt.offset.y + localToWorldCmpt.Forward * gunCmpt.offset.z;
                        var random = new Unity.Mathematics.Random ((uint) (dT * 10000));
                        for (int i = 0; i < gunCmpt.bulletNum; i++) {
                            var bulletEntity = ecb.Instantiate (entityInQueryIndex, gunCmpt.bulletPrefab);
                            var shootDir = shootInputCmpt.dir + random.NextFloat3Direction () * 0.3f;
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootDir, math.up ()) });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + offset });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootDir * gunCmpt.bulletShootSpeed });
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                        }
                    }
                } else weaponStateCmpt.recoveryLeftTime -= dT;
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var tankGunJobHandle = new TankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var doubleTankGunJobHandle = new DoubleTankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, tankGunJobHandle);
            var shotgunJobHandle = new ShotgunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, doubleTankGunJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (shotgunJobHandle);
            return shotgunJobHandle;
        }
    }
}