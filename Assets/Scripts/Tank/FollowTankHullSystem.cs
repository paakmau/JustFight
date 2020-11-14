using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight.Tank {

    [UpdateInGroup (typeof (TransformSystemGroup))]
    class FollowTankHullSystem : SystemBase {

        [BurstCompile]
        struct MoveJob : IJobChunk {
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentTypeHandle<TankHullToFollow> tankToFollowType;
            [ReadOnly] public ComponentTypeHandle<Rotation> rotationType;
            [ReadOnly] public ComponentTypeHandle<NonUniformScale> nonUniformScaleType;
            public ComponentTypeHandle<LocalToWorld> localToWorldType;
            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset) {
                var chunkTankToFollow = chunk.GetNativeArray (tankToFollowType);
                var chunkRotation = chunk.GetNativeArray (rotationType);
                var chunkNonUniformScale = chunk.GetNativeArray (nonUniformScaleType);
                var chunkLocalToWorld = chunk.GetNativeArray (localToWorldType);
                var hasNonUniformScale = chunk.Has (nonUniformScaleType);
                for (int i = 0; i < chunk.Count; i++) {
                    var translation = translationFromEntity[chunkTankToFollow[i].entity].Value + chunkTankToFollow[i].offset;
                    var rotation = chunkRotation[i].Value;
                    if (hasNonUniformScale) {
                        chunkLocalToWorld[i] = new LocalToWorld { Value = math.mul (new float4x4 (rotation, translation), float4x4.Scale (chunkNonUniformScale[i].Value)) };
                    } else
                        chunkLocalToWorld[i] = new LocalToWorld { Value = new float4x4 (rotation, translation) };
                }
            }
        }

        private EntityQuery group;

        protected override void OnCreate () {
            group = GetEntityQuery (new EntityQueryDesc {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<TankHullToFollow> (),
                        ComponentType.ReadOnly<Rotation> (),
                        typeof (LocalToWorld)
                }
            });
        }

        protected override void OnUpdate () {
            Dependency = new MoveJob {
                translationFromEntity = GetComponentDataFromEntity<Translation> (true),
                    tankToFollowType = GetComponentTypeHandle<TankHullToFollow> (true),
                    rotationType = GetComponentTypeHandle<Rotation> (true),
                    nonUniformScaleType = GetComponentTypeHandle<NonUniformScale> (true),
                    localToWorldType = GetComponentTypeHandle<LocalToWorld> ()
            }.ScheduleParallel (group, Dependency);
        }
    }
}