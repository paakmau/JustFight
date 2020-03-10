using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Tank {

    class HealthBarSystem : SystemBase {

        [BurstCompile]
        struct ScaleHealthBarJob : IJobForEach<HealthBar, TankHullToFollow, NonUniformScale> {
            [ReadOnly] public ComponentDataFromEntity<HealthPoint> healthCmpts;
            public void Execute ([ReadOnly] ref HealthBar Cmpt, [ReadOnly] ref TankHullToFollow tankToFollowCmpt, ref NonUniformScale scaleCmpt) {
                scaleCmpt.Value.y = (float) healthCmpts[tankToFollowCmpt.entity].value / (float) healthCmpts[tankToFollowCmpt.entity].maxValue;
                // TODO: 在模型中设定锚点，使血条起点对齐
            }
        }
        protected override void OnUpdate () {
            Dependency = new ScaleHealthBarJob { healthCmpts = GetComponentDataFromEntity<HealthPoint> (true) }.Schedule (this, Dependency);
        }
    }
}