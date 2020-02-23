using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace JustFight.Skill {

    [Serializable][WriteGroup (typeof (LocalToWorld))]
    struct Shadow : IComponentData {
        public Entity translationEntity;
        public Entity rotationEntity;
        public float3 offset;
    }

    [Serializable]
    struct ShadowTurret : IComponentData {
        public Entity turretEntity;
    }
}