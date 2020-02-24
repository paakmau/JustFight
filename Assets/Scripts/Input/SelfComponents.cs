using System;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Input {
    [Serializable]
    struct SelfHull : IComponentData { }

    [Serializable]
    struct SelfTurret : IComponentData { }

    class FollowCamera : IComponentData {
        public Transform transform;
    }
}