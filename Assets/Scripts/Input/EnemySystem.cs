using JustFight.Tank;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

namespace JustFight.Input {

    class EnemySystem : SystemBase {

        protected override void OnUpdate () {
            var dT = Time.DeltaTime;
            var enemyHullJobHandle = Entities.ForEach ((ref EnemyHull hullCmp, ref MoveInput moveInputCmpt) => {
                hullCmp.moveLeftTime -= dT;
                if (hullCmp.moveLeftTime <= 0) {
                    hullCmp.moveLeftTime += hullCmp.random.NextFloat (0.5f, 1.2f);
                    var dir = hullCmp.random.NextFloat2Direction ();
                    hullCmp.moveDirction = new float3 (dir.x, 0, dir.y);
                    moveInputCmpt.dir = hullCmp.moveDirction;
                }
            }).ScheduleParallel (Dependency);
            var enemyTurretJobHandle = Entities.ForEach ((ref EnemyTurret turretCmpt, ref AimInput aimInputCmpt) => {
                turretCmpt.rotateLeftTime -= dT;
                if (turretCmpt.rotateLeftTime <= 0) {
                    turretCmpt.rotateLeftTime += turretCmpt.random.NextFloat (0.2f, 0.4f);
                    turretCmpt.rotateDirection = turretCmpt.random.NextBool ();
                    aimInputCmpt.isCast = turretCmpt.random.NextBool ();
                    // To avoid floating-point error 
                    aimInputCmpt.dir.y = 0;
                    aimInputCmpt.dir = math.normalize (aimInputCmpt.dir);
                }
                aimInputCmpt.dir = math.rotate (quaternion.AxisAngle (math.up (), turretCmpt.rotateDirection ? 0.05f : -0.05f), aimInputCmpt.dir);
            }).ScheduleParallel (Dependency);
            Dependency = JobHandle.CombineDependencies (enemyHullJobHandle, enemyTurretJobHandle);
        }
    }
}