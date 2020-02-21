using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class HealthBarSystem : JobComponentSystem {

        [BurstCompile]
        struct ScaleHealthBarJob : IJobForEach<HealthBar, TankHullToFollow, NonUniformScale> {
            [ReadOnly] public ComponentDataFromEntity<Health> healthCmpts;
            public void Execute ([ReadOnly] ref HealthBar Cmpt, [ReadOnly] ref TankHullToFollow tankToFollowCmpt, ref NonUniformScale scaleCmpt) {
                scaleCmpt.Value.y = (float) healthCmpts[tankToFollowCmpt.entity].value / (float) healthCmpts[tankToFollowCmpt.entity].maxValue;
                // TODO: 添加scalePivot这类，或在模型中设定锚点，使血条起点对齐
            }
        }
        protected override JobHandle OnUpdate (Unity.Jobs.JobHandle inputDeps) {
            var scaleHealthBarJobHandle = new ScaleHealthBarJob { healthCmpts = GetComponentDataFromEntity<Health> (true) }.Schedule (this, inputDeps);
            return scaleHealthBarJobHandle;
        }
    }
}