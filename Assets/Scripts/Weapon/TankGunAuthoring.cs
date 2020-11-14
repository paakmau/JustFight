using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Weapon {

    class TankGunAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject bulletPrefab = null;
        public float bulletShootSpeed = 15;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new TankGun {
                bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    bulletShootSpeed = bulletShootSpeed
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}