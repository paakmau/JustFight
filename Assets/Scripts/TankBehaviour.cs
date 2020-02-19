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
    struct TankTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct MoveInput : IComponentData {
        public float2 dir;
    }

    [Serializable]
    struct JumpInput : IComponentData {
        public bool isJump;
    }

    [Serializable]
    struct ShootInput : IComponentData {
        public bool isShoot;
        public float2 dir;
    }

    [Serializable]
    struct SkillInput : IComponentData {
        public bool isCast;
    }

    [Serializable]
    struct JumpState : IComponentData {
        public float speed;
        public float leftRecoveryTime;
        public float recoveryTime;
    }

    [Serializable]
    struct Health : IComponentData {
        public int value;
        public int maxValue;
    }

    [Serializable]
    struct HealthBarPrefab : IComponentData {
        public Entity entity;
    }

    [Serializable]
    struct HealthBarInstance : IComponentData {
        public Entity entity;
    }

    [RequiresEntityConversion]
    class TankBehaviour : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public int teamId = 0;
        public float jumpSpeed = 10;
        public float jumpRecoveryTime = 1.25f;
        public GameObject healthBarPrefab = null;
        public int maxHealth = 100;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new TankTeam { id = teamId });
            dstManager.AddComponent<MoveInput> (entity);
            dstManager.AddComponent<JumpInput> (entity);
            dstManager.AddComponent<ShootInput> (entity);
            dstManager.AddComponent<SkillInput> (entity);
            dstManager.AddComponentData (entity, new JumpState { speed = jumpSpeed, recoveryTime = jumpRecoveryTime });
            dstManager.AddComponentData (entity, new Health { maxValue = maxHealth, value = maxHealth });
            dstManager.AddComponentData (entity, new HealthBarPrefab { entity = conversionSystem.GetPrimaryEntity (healthBarPrefab) });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (healthBarPrefab);
        }
    }

    class TankSystem : JobComponentSystem {

        [BurstCompile]
        struct InstantiateHealthBarJob : IJobForEachWithEntity<HealthBarPrefab, Translation> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref HealthBarPrefab prefabCmpt, [ReadOnly] ref Translation translationCmpt) {
                var healthBarEntity = ecb.Instantiate (entityInQueryIndex, prefabCmpt.entity);
                ecb.SetComponent (entityInQueryIndex, healthBarEntity, new Translation { Value = translationCmpt.Value + new float3 (-0.8f, 0, 0) });
                ecb.SetComponent (entityInQueryIndex, healthBarEntity, new TankToFollow { entity = entity });
                ecb.RemoveComponent<HealthBarPrefab> (entityInQueryIndex, entity);
                ecb.AddComponent (entityInQueryIndex, entity, new HealthBarInstance { entity = healthBarEntity });
            }
        }

        [BurstCompile]
        struct DestroyTankJob : IJobForEachWithEntity<Health, HealthBarInstance> {
            public EntityCommandBuffer.Concurrent ecb;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref Health healthCmpt, [ReadOnly] ref HealthBarInstance instanceCmpt) {
                if (healthCmpt.value <= 0) {
                    ecb.DestroyEntity (entityInQueryIndex, instanceCmpt.entity);
                    ecb.DestroyEntity (entityInQueryIndex, entity);
                }
            }
        }

        [BurstCompile]
        struct JumpTankJob : IJobForEach<JumpInput, JumpState, PhysicsVelocity> {
            public float dT;
            public void Execute ([ReadOnly] ref JumpInput jumpInputCmpt, ref JumpState jumpStateCmpt, ref PhysicsVelocity velocityCmpt) {
                if (jumpStateCmpt.leftRecoveryTime > 0) {
                    jumpStateCmpt.leftRecoveryTime -= dT;
                } else {
                    if (jumpInputCmpt.isJump) {
                        velocityCmpt.Linear.y += jumpStateCmpt.speed;
                        jumpStateCmpt.leftRecoveryTime = jumpStateCmpt.recoveryTime;
                    }
                }
            }
        }

        [BurstCompile]
        struct MoveTankJob : IJobForEach<MoveInput, Rotation, PhysicsVelocity> {
            public void Execute ([ReadOnly] ref MoveInput moveInputCmpt, ref Rotation rotationCmpt, ref PhysicsVelocity velocityCmpt) {
                var dir = new float3 (moveInputCmpt.dir.x, 0, moveInputCmpt.dir.y);
                if (dir.x != 0 || dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (dir, math.up ());
                dir *= 4;
                dir.y = velocityCmpt.Linear.y;
                velocityCmpt.Linear = dir;
            }
        }

        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate () {
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var instantiateHealthBarJobHandle = new InstantiateHealthBarJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var destroyTankJobHandle = new DestroyTankJob { ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent () }.Schedule (this, inputDeps);
            var jumpTankJobHandle = new JumpTankJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
            var moveTankJobHandle = new MoveTankJob ().Schedule (this, jumpTankJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (instantiateHealthBarJobHandle);
            entityCommandBufferSystem.AddJobHandleForProducer (destroyTankJobHandle);
            return JobHandle.CombineDependencies (instantiateHealthBarJobHandle, destroyTankJobHandle, moveTankJobHandle);
        }
    }
}