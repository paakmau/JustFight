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
    struct TankToFollow : IComponentData {
        public Entity entity;
    }

    [RequiresEntityConversion]
    class HealthBarBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        // float3 scalePivot = new float3 ();
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankToFollow> (entity);
            // TODO: 添加ScalePivot这类东西
        }
    }

    class HealthBarSystem : JobComponentSystem {

        [BurstCompile]
        struct MoveHealthBarJob : IJobForEach<TankToFollow, Translation> {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> localToWorldCmpts;
            public void Execute ([ReadOnly] ref TankToFollow tankToFollowCmpt, ref Translation translationCmpt) {
                translationCmpt.Value = localToWorldCmpts[tankToFollowCmpt.entity].Position + new float3 (-0.8f, 0, 0);
            }
        }

        [BurstCompile]
        struct ScaleHealthBarJob : IJobForEach<TankToFollow, NonUniformScale> {
            [ReadOnly] public ComponentDataFromEntity<Health> healthCmpts;
            public void Execute ([ReadOnly] ref TankToFollow tankToFollowCmpt, ref NonUniformScale scaleCmpt) {
                scaleCmpt.Value.z = (float) healthCmpts[tankToFollowCmpt.entity].value / (float) healthCmpts[tankToFollowCmpt.entity].maxValue;
            }
        }
        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var moveHealthBarJobHandle = new MoveHealthBarJob { localToWorldCmpts = GetComponentDataFromEntity<LocalToWorld> (true) }.Schedule (this, inputDeps);
            var scaleHealthBarJobHandle = new ScaleHealthBarJob { healthCmpts = GetComponentDataFromEntity<Health> (true) }.Schedule (this, inputDeps);
            return JobHandle.CombineDependencies (moveHealthBarJobHandle, scaleHealthBarJobHandle);
        }
    }
}