using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    [UpdateInGroup (typeof (TransformSystemGroup))]
    class ShadowSystem : JobComponentSystem {

        [BurstCompile]
        struct ShadowJob : IJobChunk {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;
            [ReadOnly] public ArchetypeChunkComponentType<Shadow> shadowType;
            public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;
            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset) {
                var chunkShadow = chunk.GetNativeArray (shadowType);
                var chunkLocalToWorld = chunk.GetNativeArray (localToWorldType);
                for (int i = 0; i < chunk.Count; i++) {
                    var translation = translationFromEntity[chunkShadow[i].translationEntity].Value;
                    var rotation = rotationFromEntity[chunkShadow[i].rotationEntity].Value;
                    chunkLocalToWorld[i] = new LocalToWorld { Value = math.mul (float4x4.Translate (chunkShadow[i].offset), new float4x4 (rotation, translation)) };
                }
            }
        }

        private EntityQuery group;

        protected override void OnCreate () {
            group = GetEntityQuery (new EntityQueryDesc {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<Shadow> (),
                        typeof (LocalToWorld)
                }
            });
        }

        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var moveJobHandle = new ShadowJob {
                translationFromEntity = GetComponentDataFromEntity<Translation> (true),
                    rotationFromEntity = GetComponentDataFromEntity<Rotation> (true),
                    shadowType = GetArchetypeChunkComponentType<Shadow> (true),
                    localToWorldType = GetArchetypeChunkComponentType<LocalToWorld> ()
            }.Schedule (group, inputDeps);
            return moveJobHandle;
        }
    }
}