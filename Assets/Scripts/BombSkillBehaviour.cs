using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {

    [Serializable]
    struct BombSkill : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
        public float forwardOffset;
        public float radius;
        public Entity bulletPrefab;
        public int bulletNum;
    }

    [RequiresEntityConversion]
    class BombSkillBehaviour : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 8;
        public float bombForwarOffset = 0;
        public float bombRadius = 3;
        public int bulletNum = 15;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new BombSkill { recoveryTime = recoveryTime, forwardOffset = bombForwarOffset, radius = bombRadius, bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab), bulletNum = bulletNum });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }

    class BombSkillSystem : JobComponentSystem {

        [BurstCompile]
        struct SkillJob : IJobForEachWithEntity<TankTeam, ShootInput, SkillInput, BombSkill, LocalToWorld> {
            public EntityCommandBuffer.Concurrent ecb;
            public float dT;
            public Unity.Mathematics.Random rand;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref TankTeam teamCmpt, [ReadOnly] ref ShootInput shootInputCmpt, [ReadOnly] ref SkillInput skillInputCmpt, ref BombSkill skillCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
                if (skillCmpt.recoveryLeftTime <= 0) {
                    if (skillInputCmpt.isCast) {
                        skillCmpt.recoveryLeftTime += skillCmpt.recoveryTime;
                        var offset = shootInputCmpt.dir * skillCmpt.forwardOffset;
                        var center = localToWorldCmpt.Position + new float3 (offset.x, 6, offset.y);
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