using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Bullet {

    [Serializable]
    struct BulletTeam : IComponentData {
        public Entity hull;
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