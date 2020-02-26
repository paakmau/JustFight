using JustFight.Bullet;
using JustFight.Tank;
using JustFight.Weapon;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Skill {

    [UpdateInGroup (typeof (TransformSystemGroup))]
    class ShadowSystem : JobComponentSystem {

        [BurstCompile]
        struct ShadowMoveJob : IJobChunk {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;
            [ReadOnly] public ArchetypeChunkEntityType entityType;
            [ReadOnly] public ArchetypeChunkComponentType<Shadow> shadowType;
            public ArchetypeChunkComponentType<LocalToWorld> localToWorldType;
            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset) {
                var chunkEntity = chunk.GetNativeArray (entityType);
                var chunkShadow = chunk.GetNativeArray (shadowType);
                var chunkLocalToWorld = chunk.GetNativeArray (localToWorldType);
                for (int i = 0; i < chunk.Count; i++) {
                    var isTranslationEntityValid = translationFromEntity.Exists (chunkShadow[i].translationEntity);
                    var isRotationEntityValid = rotationFromEntity.Exists (chunkShadow[i].rotationEntity);
                    if (isTranslationEntityValid && isRotationEntityValid) {
                        var translation = translationFromEntity[chunkShadow[i].translationEntity].Value;
                        var rotation = rotationFromEntity[chunkShadow[i].rotationEntity].Value;
                        chunkLocalToWorld[i] = new LocalToWorld { Value = math.mul (float4x4.Translate (chunkShadow[i].offset), new float4x4 (rotation, translation)) };
                    } else {
                        ecb.DestroyEntity (chunkIndex, chunkEntity[i]);
                    }
                }
            }
        }

        [BurstCompile]
        struct ShadowShootJob : IJobForEachWithEntity<ShadowTurret, AimInput> {
            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly] public ComponentDataFromEntity<ShadowSkill> shadowSkillFromEntity;
            [ReadOnly] public float dT;
            public void Execute (Entity entity, int entityInQueryIndex, [ReadOnly] ref ShadowTurret shadowTurretCmpt, ref AimInput aimInputCmpt) {
                var isTurretEntityValid = shadowSkillFromEntity.Exists (shadowTurretCmpt.turretEntity);
                if (!isTurretEntityValid)
                    ecb.DestroyEntity (entityInQueryIndex, entity);
                else
                    aimInputCmpt.dir = shadowSkillFromEntity[shadowTurretCmpt.turretEntity].aimDir;
            }
        }

        private EntityQuery group;
        private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate () {
            group = GetEntityQuery (new EntityQueryDesc {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<Shadow> (),
                        typeof (LocalToWorld)
                }
            });
            entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
        }

        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var moveJobHandle = new ShadowMoveJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (),
                    translationFromEntity = GetComponentDataFromEntity<Translation> (true),
                    rotationFromEntity = GetComponentDataFromEntity<Rotation> (true),
                    entityType = GetArchetypeChunkEntityType (),
                    shadowType = GetArchetypeChunkComponentType<Shadow> (true),
                    localToWorldType = GetArchetypeChunkComponentType<LocalToWorld> ()
            }.Schedule (group, inputDeps);
            var shadowShootJobHandle = new ShadowShootJob {
                ecb = entityCommandBufferSystem.CreateCommandBuffer ().ToConcurrent (),
                    shadowSkillFromEntity = GetComponentDataFromEntity<ShadowSkill> (),
                    dT = Time.DeltaTime
            }.Schedule (this, moveJobHandle);
            return shadowShootJobHandle;
        }
    }
}