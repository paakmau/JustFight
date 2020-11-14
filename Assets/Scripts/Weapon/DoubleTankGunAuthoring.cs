using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Weapon {

    class DoubleTankGunAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject bulletPrefab = null;
        public float bulletShootSpeed = 15;
        public float offsetAX = 0.4f;
        public float offsetBX = -0.4f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new DoubleTankGun {
                bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    bulletShootSpeed = bulletShootSpeed,
                    offsetAX = offsetAX,
                    offsetBX = offsetBX
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}