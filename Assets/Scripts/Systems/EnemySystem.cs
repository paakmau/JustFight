using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

namespace JustFight {

    class EnemySystem : JobComponentSystem {
        [BurstCompile]
        struct EnemyHullJob : IJobForEach<EnemyHull, MoveInput> {
            public float dT;
            public void Execute (ref EnemyHull hullCmp, ref MoveInput moveInputCmpt) {
                hullCmp.moveLeftTime -= dT;
                if (hullCmp.moveLeftTime <= 0) {
                    hullCmp.moveLeftTime += hullCmp.random.NextFloat (0.5f, 1.2f);
                    var dir = hullCmp.random.NextFloat2Direction ();
                    hullCmp.moveDirction = new float3 (dir.x, 0, dir.y);
                    moveInputCmpt.dir = hullCmp.moveDirction;
                }
            }
        }

        [BurstCompile]
        struct EnemyTurretJob : IJobForEach<EnemyTurret, ShootInput, SkillInput> {
            public float dT;
            public void Execute (ref EnemyTurret turretCmpt, ref ShootInput shootInputCmpt, ref SkillInput skillInputCmpt) {
                turretCmpt.rotateLeftTime -= dT;
                if (turretCmpt.rotateLeftTime <= 0) {
                    turretCmpt.rotateLeftTime += turretCmpt.random.NextFloat (0.2f, 0.4f);
                    shootInputCmpt.isShoot = true;
                    skillInputCmpt.isCast = true;
                    shootInputCmpt.dir.y = 0;
                    shootInputCmpt.dir = math.normalize (shootInputCmpt.dir);
                }
                shootInputCmpt.dir = math.rotate (quaternion.AxisAngle (math.up (), turretCmpt.rotateDirection ? 0.05f : -0.05f), shootInputCmpt.dir);
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            return JobHandle.CombineDependencies (new EnemyHullJob { dT = Time.DeltaTime }.Schedule (this, inputDeps), new EnemyTurretJob { dT = Time.DeltaTime }.Schedule (this, inputDeps));
        }
    }
}