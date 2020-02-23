using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class SpraySkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public float skillShootSpeed = 16;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new SpraySkill {
                skillShootRecoveryTime = skillShootRecoveryTime,
                    skillLastTime = skillLastTime,
                    bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    skillShootSpeed = skillShootSpeed
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}