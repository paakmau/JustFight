using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight {

    [Serializable]
    struct TankTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct MoveInput : IComponentData {
        public float2 dir;
    }

    [Serializable]
    struct JumpInput : IComponentData {
        public bool isJump;
    }

    [Serializable]
    struct ShootInput : IComponentData {
        public bool isShoot;
        public float2 dir;
    }

    [Serializable]
    struct SkillInput : IComponentData {
        public bool isCast;
    }

    [Serializable]
    struct JumpState : IComponentData {
        public float speed;
        public float leftRecoveryTime;
        public float recoveryTime;
    }

    [Serializable]
    struct Health : IComponentData {
        public int value;
        public int maxValue;
    }

    [Serializable]
    struct TankTurretPrefab : IComponentData {
        public Entity entity;
    }

    [Serializable]
    struct TankTurretInstance : IComponentData {
        public Entity entity;
    }

    [Serializable]
    struct HealthBarPrefab : IComponentData {
        public Entity entity;
    }

    [Serializable]
    struct HealthBarInstance : IComponentData {
        public Entity entity;
    }
}