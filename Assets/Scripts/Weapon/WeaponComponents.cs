using System;
using Unity.Entities;

namespace JustFight.Weapon {

    [Serializable]
    struct WeaponState : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
        public bool isShootTrigger;
    }

    [Serializable]
    struct TankGun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
    }

    [Serializable]
    struct DoubleTankGun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
        public float offsetAX;
        public float offsetBX;
    }

    [Serializable]
    struct Shotgun : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
        public int bulletNum;
    }
}