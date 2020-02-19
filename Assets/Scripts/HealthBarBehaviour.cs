using System;
using Unity.Entities;
using UnityEngine;

namespace JustFight {
    [Serializable]
    struct TankToFollow : IComponentData {
        public Entity entity;
    }

    [RequiresEntityConversion]
    class HealthBarBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        // float3 scalePivot = new float3 ();
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankToFollow> (entity);
            // TODO: 添加ScalePivot这类东西
        }
    }
}