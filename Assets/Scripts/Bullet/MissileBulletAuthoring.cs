using Unity.Entities;
using UnityEngine;

namespace JustFight.Bullet {

    class MissileBulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<MissileBullet> (entity);
        }
    }
}