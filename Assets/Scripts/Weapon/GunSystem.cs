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

    class GunSystem : JobComponentSystem {

        [BurstCompile]
        struct ShootJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, GunState, GunBullet, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, ref GunState gunStateCmpt, [ReadOnly] ref GunBullet bulletCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (gunStateCmpt.recoveryLeftTime <= 0) {
                    if (shootInputCmpt.isShoot) {
                        gunStateCmpt.recoveryLeftTime += gunStateCmpt.recoveryTime;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, bulletCmpt.bulletPrefab);
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ()) });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + localToWorldCmpt.Forward * 1.7f });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = shootInputCmpt.dir * bulletCmpt.bulletShootSpeed });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                    }
                } else gunStateCmpt.recoveryLeftTime -= dT;
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var shootJobHandle = new ShootJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (), dT = Time.DeltaTime }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (shootJobHandle);
            return shootJobHandle;
        }
    }
}