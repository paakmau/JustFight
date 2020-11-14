using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    class BombSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 6;
        public float bombForwarOffset = 0;
        public float bombRadius = 8;
        public int bulletNum = 30;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Skill { recoveryLeftTime = recoveryTime, recoveryTime = recoveryTime });
            dstManager.AddComponentData (entity, new BombSkill { forwardOffset = bombForwarOffset, radius = bombRadius, bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab), bulletNum = bulletNum });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}