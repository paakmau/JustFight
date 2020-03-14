using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight.Tank {

    class TankTurretSystem : SystemBase {

        protected override void OnUpdate () {
            // 根据输入旋转炮台
            Dependency = Entities.ForEach ((ref Rotation rotationCmpt, in AimInput shootInputCmpt) => {
                if (shootInputCmpt.dir.x != 0 || shootInputCmpt.dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ());
            }).ScheduleParallel (Dependency);
        }
    }
}