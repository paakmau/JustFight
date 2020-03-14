using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace JustFight.Tank {

    class HealthBarSystem : SystemBase {

        protected override void OnUpdate () {
            // 根据血量缩放血条
            var healthFromEntity = GetComponentDataFromEntity<HealthPoint> (true);
            Dependency = Entities.WithReadOnly(healthFromEntity).ForEach ((ref NonUniformScale scaleCmpt, in HealthBar Cmpt, in TankHullToFollow tankToFollowCmpt) => {
                scaleCmpt.Value.z = (float) healthFromEntity[tankToFollowCmpt.entity].value / (float) healthFromEntity[tankToFollowCmpt.entity].maxValue;
            }).ScheduleParallel (Dependency);
        }
    }
}