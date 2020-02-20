using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class BombSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 8;
        public float bombForwarOffset = 0;
        public float bombRadius = 3;
        public int bulletNum = 15;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new BombSkill { recoveryTime = recoveryTime, forwardOffset = bombForwarOffset, radius = bombRadius, bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab), bulletNum = bulletNum });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}