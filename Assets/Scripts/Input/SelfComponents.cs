using System;
using Unity.Entities;

namespace JustFight.Input {
    [Serializable]
    struct SelfHull : IComponentData { }

    [Serializable]
    struct SelfTurret : IComponentData { }
}