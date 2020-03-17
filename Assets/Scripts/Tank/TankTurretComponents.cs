using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Tank {

    [Serializable]
    struct TankTurretTeam : IComponentData {
        public int id;
        public Entity hull;
    }

    [Serializable]
    struct AimInput : IComponentData {
        public bool isCast;
        public float3 dir;
    }

}