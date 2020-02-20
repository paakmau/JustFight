using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {

    [Serializable]
    struct BulletTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct BulletDamage : IComponentData {
        public int value;
    }

    [Serializable]
    struct BulletDestroyTime : IComponentData {
        public float value;
    }

    [Serializable]
    struct MissileBullet : IComponentData { }

    [Serializable]
    struct WaveBulletState : IComponentData {
        public float3 forward;
        public int factor;
        public float leftRecoveryTime;
        public float recoveryTime;
    }

}