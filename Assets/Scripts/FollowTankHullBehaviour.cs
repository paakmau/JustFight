using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class FollowTankHullBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankToFollow> (entity);
        }
    }

    // [UpdateAfter (typeof (TankHullSystem))]
    [UpdateInGroup (typeof (TransformSystemGroup))]
    class FollowTankHullSystem : JobComponentSystem {

        [BurstCompile]
        struct MoveJob : IJobChunk {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ArchetypeChunkComponentType<TankToFollow> tankToFollowType;
            [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotationType;
            [ReadOnly] public ArchetypeChunkComponentType<NonUniformScale> nonUniformScaleType;
            public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;
            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset) {
                var chunkTankToFollow = chunk.GetNativeArray (tankToFollowType);
                var chunkRotation = chunk.GetNativeArray (rotationType);
                var chunkNonUniformScale = chunk.GetNativeArray (nonUniformScaleType);
                var chunkLocalToWorld = chunk.GetNativeArray (localToWorldType);
                for (int i = 0; i < chunk.Count; i++) {
                    var tankEntitiy = chunkTankToFollow[i].entity;
                    var translation = translationFromEntity[tankEntitiy].Value + chunkTankToFollow[i].offset;
                    var rotation = chunkRotation[i].Value;
                    var scale = float4x4.Scale (chunkNonUniformScale[i].Value);
                    chunkLocalToWorld[i] = new LocalToWorld { Value = math.mul (new float4x4 (rotation, translation), scale) };
                }
            }
        }

        private EntityQuery m_Group;

        protected override void OnCreate () {
            m_Group = GetEntityQuery (
                ComponentType.ReadOnly<TankToFollow> (),
                ComponentType.ReadOnly<Rotation> (),
                ComponentType.ReadOnly<NonUniformScale> (),
                typeof (LocalToWorld)
            );
        }

        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var moveJobHandle = new MoveJob {
                translationFromEntity = GetComponentDataFromEntity<Translation> (true),
                    tankToFollowType = GetArchetypeChunkComponentType<TankToFollow> (true),
                    rotationType = GetArchetypeChunkComponentType<Rotation> (true),
                    nonUniformScaleType = GetArchetypeChunkComponentType<NonUniformScale> (true),
                    localToWorldType = GetArchetypeChunkComponentType<LocalToWorld> ()
            }.Schedule (m_Group, inputDeps);
            return moveJobHandle;
        }
    }
}