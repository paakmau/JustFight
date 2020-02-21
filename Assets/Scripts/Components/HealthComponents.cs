using System;
using Unity.Entities;

namespace JustFight {

    [Serializable]
    struct Health : IComponentData {
        public int value;
        public int maxValue;
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