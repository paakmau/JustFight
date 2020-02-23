using Unity.Entities;
using UnityEngine;

namespace JustFight.Bullet {

    [RequiresEntityConversion]
    class BulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public int damage = 15;
        public float destroyTime = 10;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<BulletTeam> (entity);
            dstManager.AddComponentData (entity, new BulletDamage { value = damage });
            dstManager.AddComponentData (entity, new BulletDestroyTime { value = destroyTime });
        }
    }
}