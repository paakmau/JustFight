using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    [UpdateInGroup (typeof (TransformSystemGroup))]
    class FollowTankHullSystem : JobComponentSystem {

        [BurstCompile]
        struct MoveJob : IJobChunk {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ArchetypeChunkComponentType<TankHullToFollow> tankToFollowType;
            [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotationType;
            public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;
            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset) {
                var chunkTankToFollow = chunk.GetNativeArray (tankToFollowType);
                var chunkRotation = chunk.GetNativeArray (rotationType);
                var chunkLocalToWorld = chunk.GetNativeArray (localToWorldType);
                for (int i = 0; i < chunk.Count; i++) {
                    var tankEntitiy = chunkTankToFollow[i].entity;
                    var translation = translationFromEntity[tankEntitiy].Value + chunkTankToFollow[i].offset;
                    var rotation = chunkRotation[i].Value;
                    chunkLocalToWorld[i] = new LocalToWorld { Value = new float4x4 (rotation, translation) };
                }
            }
        }

        private EntityQuery group;

        protected override void OnCreate () {
            group = GetEntityQuery (
                ComponentType.ReadOnly<TankHullToFollow> (),
                ComponentType.ReadOnly<Rotation> (),
                typeof (LocalToWorld)
            );
        }

        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var moveJobHandle = new MoveJob {
                translationFromEntity = GetComponentDataFromEntity<Translation> (true),
                    tankToFollowType = GetArchetypeChunkComponentType<TankHullToFollow> (true),
                    rotationType = GetArchetypeChunkComponentType<Rotation> (true),
                    localToWorldType = GetArchetypeChunkComponentType<LocalToWorld> ()
            }.Schedule (group, inputDeps);
            return moveJobHandle;
        }
    }
}