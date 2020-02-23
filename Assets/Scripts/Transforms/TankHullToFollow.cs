using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight.Transforms {
    [Serializable][WriteGroup (typeof (LocalToWorld))]
    struct TankHullToFollow : IComponentData {
        public Entity entity;
        public float3 offset;
    }
}