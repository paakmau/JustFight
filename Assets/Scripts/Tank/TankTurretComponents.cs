using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Tank {

    [Serializable]
    struct TankTurretTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct ShootInput : IComponentData {
        public bool isShoot;
        public float3 dir;
    }

    [Serializable]
    struct SkillInput : IComponentData {
        public bool isCast;
    }
}