using System;
using Unity.Entities;

namespace JustFight.Weapon {

    [Serializable]
    struct GunBullet : IComponentData {
        public Entity bulletPrefab;
        public float bulletShootSpeed;
    }

    [Serializable]
    struct GunState : IComponentData {
        public float recoveryLeftTime;
        public float recoveryTime;
    }

}