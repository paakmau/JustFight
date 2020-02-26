using Unity.Entities;
using UnityEngine;

namespace JustFight.FollowCamera {

    class FollowCameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var followCameraTransformCmpt = new FollowCameraTransform { transform = transform };
            dstManager.AddComponentData (entity, followCameraTransformCmpt);
        }
    }
}