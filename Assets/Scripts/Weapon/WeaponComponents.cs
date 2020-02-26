using System;
using Unity.Entities;
using Unity.Mathematics;

namespace JustFight.Weapon {

    [Serializable]
    struct WeaponState : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
    }

    [Serializable]
    struct TankGun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
        public float3 offset;
    }

    [Serializable]
    struct DoubleTankGun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
        public float3 offsetA;
        public float3 offsetB;
    }

    [Serializable]
    struct Shotgun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
        public int bulletNum;
        public float3 offset;
    }
}