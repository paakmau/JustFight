using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight {

    [Serializable]
    struct EnemyAction : IComponentData {
        public Unity.Mathematics.Random random;
        public float3 moveDirction;
        public float moveLeftTime;
    }
}