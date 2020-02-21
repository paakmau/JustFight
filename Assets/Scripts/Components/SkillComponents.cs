using System;
using Unity.Entities;

namespace JustFight {

    [Serializable]
    struct Skill : IComponentData {
        public float leftTime;
        public float recoveryTime;
        public bool isCastTrigger;
    }

    [Serializable]
    struct SpraySkill : IComponentData {
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
        public Entity shadowPrefab;
        public float skillLastTime;
    }
}