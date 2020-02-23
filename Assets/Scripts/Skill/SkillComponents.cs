using System;
using Unity.Entities;

namespace JustFight.Skill {

    [Serializable]
    struct Skill : IComponentData {
        public float leftTime;
        public float recoveryTime;
        public bool isCastTrigger;
    }

    [Serializable]
    struct BurstSkill : IComponentData {
        public float skillShootRecoveryleftTime;
        public float skillShootRecoveryTime;
        public float skillShootSpeed;
        public float skillLeftTime;
        public float skillLastTime;
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
        public float skillLeftTime;
        public float skillLastTime;
        public Entity shadowHullInstanceA;
        public Entity shadowHullInstanceB;
        public Entity shadowTurretInstanceA;
        public Entity shadowTurretInstanceB;
    }

    [Serializable]
    struct ShotgunBurstSkill : IComponentData {
        public float skillShootRecoveryleftTime;
        public float skillShootRecoveryTime;
        public float skillShootSpeed;
        public float skillLeftTime;
        public float skillLastTime;
        public int bulletNumPerShoot;
        public Entity bulletPrefab;
    }
}