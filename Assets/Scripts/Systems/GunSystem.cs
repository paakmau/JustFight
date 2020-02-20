using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class GunSystem : JobComponentSystem {

        [BurstCompile]
        struct ShootJob : IJobForEachWithEntity<TankTeam, ShootInput, GunState, GunBullet, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, ref GunState gunStateCmpt, [ReadOnly] ref GunBullet bulletCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (gunStateCmpt.recoveryLeftTime <= 0) {
                    if (shootInputCmpt.isShoot) {
                        gunStateCmpt.recoveryLeftTime += gunStateCmpt.recoveryTime;
                        var bulletEntity = ecb.Instantiate (entityInQueryIndex, bulletCmpt.bulletPrefab);
                        var forwardDir = new float3 (shootInputCmpt.dir.x, 0, shootInputCmpt.dir.y);
                        // TODO: 子弹模型的方向反了
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Rotation { Value = quaternion.LookRotation (forwardDir, math.up ()) });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = localToWorldCmpt.Position + forwardDir * 1f });
                        ecb.SetComponent (entityInQueryIndex, bulletEntity, new PhysicsVelocity { Linear = forwardDir * bulletCmpt.bulletShootSpeed });
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