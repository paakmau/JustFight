using System;
using Unity.Entities;
using UnityEngine;

namespace JustFight {
    [Serializable]
    struct Self : IComponentData { }

    class FollowCamera : IComponentData {
        public Transform transform;
    }
}