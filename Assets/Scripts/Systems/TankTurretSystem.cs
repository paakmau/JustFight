using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace JustFight {

    class TankTurretSystem : JobComponentSystem {

        [BurstCompile]
        struct RotateTurretJob : IJobForEach<ShootInput, Rotation> {
            public void Execute ([ReadOnly] ref ShootInput shootInputCmpt, ref Rotation rotationCmpt) {
                if (shootInputCmpt.dir.x != 0 || shootInputCmpt.dir.z != 0)
                    rotationCmpt.Value = quaternion.LookRotation (shootInputCmpt.dir, math.up ());
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            return new RotateTurretJob ().Schedule (this, inputDeps);
        }
    }
}