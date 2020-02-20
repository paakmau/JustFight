using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class SelfHullAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public Transform followCameraTransform = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new SelfHull ());
            dstManager.AddComponentObject (entity, new FollowCamera { transform = followCameraTransform });
        }
    }
}