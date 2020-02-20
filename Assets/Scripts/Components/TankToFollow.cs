using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight {
    [Serializable][WriteGroup (typeof (LocalToWorld))]
    struct TankToFollow : IComponentData {
        public Entity entity;
        public float3 offset;
    }
}