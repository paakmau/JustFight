using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Tank {

    [Serializable]
    struct TankHullTeam : IComponentData {
        public int id;
    }

    [Serializable]
    struct MoveInput : IComponentData {
        public float3 dir;
    }

    [Serializable]
    struct JumpInput : IComponentData {
        public bool isJump;
    }

    [Serializable]
    struct MoveSpeed : IComponentData {
        public float value;
    }

    [Serializable]
    struct JumpState : IComponentData {
        public float speed;
        public float leftRecoveryTime;
        public float recoveryTime;
    }

    [Serializable]
    struct TankTurretInstance : IComponentData {
        public Entity entity;
    }
}