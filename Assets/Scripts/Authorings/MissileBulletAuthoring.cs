using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class MissileBulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<MissileBullet> (entity);
        }
    }
}