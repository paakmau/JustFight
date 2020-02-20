using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class BombSkillSystem : JobComponentSystem {

        [BurstCompile]
        struct SkillJob : IJobForEachWithEntity<TankTurretTeam, ShootInput, SkillInput, BombSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public Unity.Mathematics.Random rand;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTurretTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, [ReadOnly] ref SkillInput skillInputCmpt, ref BombSkill skillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (skillCmpt.recoveryLeftTime <= 0) {
                    if (skillInputCmpt.isCast) {
                        skillCmpt.recoveryLeftTime += skillCmpt.recoveryTime;
                        var offset = shootInputCmpt.dir * skillCmpt.forwardOffset;
                        var center = localToWorldCmpt.Position + offset + new float3 (0, 6, 0);
                        for (int i = 0; i < skillCmpt.bulletNum; i++) {
                            var bulletEntity = ecb.Instantiate (entityInQueryIndex, skillCmpt.bulletPrefab);
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new BulletTeam { id = teamCmpt.id });
                            var randDir = (rand.NextFloat2Direction () * rand.NextFloat (skillCmpt.radius));
                            ecb.SetComponent (entityInQueryIndex, bulletEntity, new Translation { Value = center + new float3 (randDir.x, 0, randDir.y) });
                        }
                    }
                } else skillCmpt.recoveryLeftTime -= dT;
            }
        }
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var skillJobHandle = new SkillJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (),
                    dT = Time.DeltaTime,
                    rand = new Unity.Mathematics.Random ((uint) (Time.DeltaTime * 10000))
            }.Schedule (this, inputDeps);
            entityCommandBufferSystem.AddJobHandleForProducer (skillJobHandle);
            return skillJobHandle;
        }
    }
}