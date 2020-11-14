using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Weapon {

    class ShotgunAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject bulletPrefab = null;
        public float bulletShootSpeed = 15;
        public int bulletNum = 8;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Shotgun {
                bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    bulletShootSpeed = bulletShootSpeed,
                    bulletNum = bulletNum
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}