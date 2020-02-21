using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight {

    [Serializable]
    struct EnemyHull : IComponentData {
        public Unity.Mathematics.Random random;
        public float3 moveDirction;
        public float moveLeftTime;
    }

    [Serializable]
    struct EnemyTurret : IComponentData {
        public Unity.Mathematics.Random random;
        public bool rotateDirection;
        public float rotateLeftTime;
    }
}