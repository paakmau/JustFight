using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Skill {

    [Serializable]
    struct Skill : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
        public bool isCastTrigger;
        public float lastLeftTime;
        public float lastTime;
        public bool isDisableWeapon;
        public bool isEndTrigger;
    }

    [Serializable]
    struct BurstSkill : IComponentData {
        public float skillShootRecoveryleftTime;
        public float skillShootRecoveryTime;
        public float skillShootSpeed;
        public float3 offset;
        public Entity bulletPrefab;
    }

    [Serializable]
    struct BombSkill : IComponentData {
        public float forwardOffset;
        public float radius;
        public Entity bulletPrefab;
        public int bulletNum;
    }

    [Serializable]
    struct ShadowSkill : IComponentData {
        public Entity shadowHullPrefab;
        public Entity shadowTurretPrefab;
        public Entity shadowHullInstanceA;
        public Entity shadowHullInstanceB;
        public Entity shadowTurretInstanceA;
        public Entity shadowTurretInstanceB;
        public float3 aimDir;
    }

    [Serializable]
    struct ShotgunBurstSkill : IComponentData {
        public float skillShootRecoveryleftTime;
        public float skillShootRecoveryTime;
        public float skillShootSpeed;
        public int bulletNumPerShoot;
        public float spread;
        public Entity bulletPrefab;
        public float3 offset;
        public float upRot;
        public bool isFlat;
    }
}