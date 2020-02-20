using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class GunAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject bulletPrefab = null;
        public float bulletShootSpeed = 15;
        public float shootRecoveryTime = 0.4f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new GunBullet { bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab), bulletShootSpeed = bulletShootSpeed });
            dstManager.AddComponentData (entity, new GunState { recoveryTime = shootRecoveryTime });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}