using System;
using Unity.Entities;

namespace JustFight {

    [Serializable]
    struct SpraySkill : IComponentData {
        public float skillRecoveryLeftTime;
        public float skillRecoveryTime;
        public float skillShootRecoveryleftTime;
        public float skillShootRecoveryTime;
        public float skillLeftTime;
        public float skillLastTime;
    }

    [Serializable]
    struct BombSkill : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
        public float forwardOffset;
        public float radius;
        public Entity bulletPrefab;
        public int bulletNum;
    }
}