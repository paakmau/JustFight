using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace JustFight {

    [Serializable]
    struct WaveBulletState : IComponentData {
        public float3 forward;
        public int factor;
        public float leftRecoveryTime;
        public float recoveryTime;
    }

    [RequiresEntityConversion]
    class WaveBulletBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new WaveBulletState { recoveryTime = 0.4f });
        }
    }

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