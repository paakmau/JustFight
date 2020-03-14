using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight.Tank {

    class HealthBarSystem : SystemBase {

        protected override void OnUpdate () {
            var healthFromEntity = GetComponentDataFromEntity<HealthPoint> (true);
            Dependency = Entities.WithReadOnly(healthFromEntity).ForEach ((ref NonUniformScale scaleCmpt, in HealthBar Cmpt, in TankHullToFollow tankToFollowCmpt) => {
                scaleCmpt.Value.z = (float) healthFromEntity[tankToFollowCmpt.entity].value / (float) healthFromEntity[tankToFollowCmpt.entity].maxValue;
            }).ScheduleParallel (Dependency);
        }
    }
}