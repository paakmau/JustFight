using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {

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