using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight.Tank {

    class HealthBarAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<HealthBar> (entity);
            dstManager.AddComponentData (entity, new NonUniformScale { Value = new float3 (1, 1, 1) });
        }
    }
}