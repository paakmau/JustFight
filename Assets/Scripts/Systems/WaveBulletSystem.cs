using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

namespace JustFight {

    class WaveBulletSystem : JobComponentSystem {

        [BurstCompile]
        struct WaveJob : IJobForEach<WaveBulletState, PhysicsVelocity> {
            public float dT;
            public unsafe void Execute (ref WaveBulletState stateCmpt, ref PhysicsVelocity velocityCmpt) {
                if (stateCmpt.leftRecoveryTime <= 0) {
                    stateCmpt.leftRecoveryTime = stateCmpt.recoveryTime;
                    float impulse = 5;
                    if (stateCmpt.factor == 0) {
                        stateCmpt.forward = math.normalize (math.cross (velocityCmpt.Linear, math.up ()));
                        stateCmpt.factor = new Unity.Mathematics.Random ((uint) (dT * 10000)).NextBool () ? 1 : -1;
                        stateCmpt.leftRecoveryTime /= 2f;
                        impulse /= 2f;
                    }
                    velocityCmpt.Linear += stateCmpt.forward * impulse * stateCmpt.factor;
                    stateCmpt.factor = -stateCmpt.factor;
                } else stateCmpt.leftRecoveryTime -= dT;
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps) {
            var waveJobHandle = new WaveJob {
                dT = Time.DeltaTime
            }.Schedule (this, inputDeps);
            return waveJobHandle;
        }
    }
}