using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class HealthBarBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<HealthBarTag> (entity);
        }
    }

    class HealthBarSystem : JobComponentSystem {

        [BurstCompile]
        struct ScaleHealthBarJob : IJobForEach<HealthBarTag, TankToFollow, NonUniformScale> {
            [ReadOnly] public ComponentDataFromEntity<Health> healthCmpts;
            public void Execute ([ReadOnly] ref HealthBarTag tagCmpt, [ReadOnly] ref TankToFollow tankToFollowCmpt, ref NonUniformScale scaleCmpt) {
                scaleCmpt.Value.z = (float) healthCmpts[tankToFollowCmpt.entity].value / (float) healthCmpts[tankToFollowCmpt.entity].maxValue;
                // TODO: 添加scalePivot这类，或在模型中设定锚点，使血条起点对齐
            }
        }
        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var scaleHealthBarJobHandle = new ScaleHealthBarJob { healthCmpts = GetComponentDataFromEntity<Health> (true) }.Schedule (this, inputDeps);
            return scaleHealthBarJobHandle;
        }
    }
}