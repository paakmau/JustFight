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
        struct TankGunJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, WeaponState, TankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, ref WeaponState weaponStateCmpt, [ReadOnly] ref TankGun gunCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
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
        struct DoubleTankGunJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, WeaponState, DoubleTankGun, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, ref WeaponState weaponStateCmpt, [ReadOnly] ref DoubleTankGun gunCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
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

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var tankGunJobHandle = new TankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var doubleTankGunJobHandle = new DoubleTankGunJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, tankGunJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (doubleTankGunJobHandle);
            return doubleTankGunJobHandle;
        }
    }
}