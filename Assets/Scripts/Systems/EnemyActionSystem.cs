using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace JustFight {

    class EnemyActionSystem : JobComponentSystem {
        [BurstCompile]
        struct EnemyActionJob : IJobForEach<EnemyAction, MoveInput, ShootInput, SkillInput> {
            public float dT;
            public void Execute (ref EnemyAction enemyActionCmpt, ref MoveInput moveInputCmpt, ref ShootInput shootInputCmpt, ref SkillInput skillInputCmpt) {
                enemyActionCmpt.moveLeftTime -= dT;
                if (enemyActionCmpt.moveLeftTime <= 0) {
                    enemyActionCmpt.moveLeftTime += enemyActionCmpt.random.NextFloat (0.5f, 1.2f);
                    enemyActionCmpt.moveDirction = math.normalize (enemyActionCmpt.random.NextFloat2 (new float2 (-1, -1), new float2 (1, 1)));
                }
                moveInputCmpt.dir = enemyActionCmpt.moveDirction;
                shootInputCmpt.dir = enemyActionCmpt.moveDirction;
                shootInputCmpt.isShoot = true;
                skillInputCmpt.isCast = true;
            }
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            return new EnemyActionJob { dT = Time.DeltaTime }.Schedule (this, inputDeps);
        }
    }
}