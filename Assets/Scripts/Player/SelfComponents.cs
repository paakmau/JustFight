using System;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Player {
    [Serializable]
    struct SelfHull : IComponentData { }

    [Serializable]
    struct SelfTurret : IComponentData { }

    class FollowCamera : IComponentData {
        public Transform transform;
    }
}