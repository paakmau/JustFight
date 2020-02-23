using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Weapon {

    [RequiresEntityConversion]
    class DoubleTankGunAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject bulletPrefab = null;
        public float bulletShootSpeed = 15;
        public float3 offsetA = default;
        public float3 offsetB = default;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new DoubleTankGun {
                bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    bulletShootSpeed = bulletShootSpeed,
                    offsetA = offsetA,
                    offsetB = offsetB
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}