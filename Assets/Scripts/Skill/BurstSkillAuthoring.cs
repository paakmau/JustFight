using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class BurstSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 4;
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public float skillShootSpeed = 16;
        public float offsetX = 0;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Skill { recoveryLeftTime = recoveryTime, recoveryTime = recoveryTime, lastTime = skillLastTime, isDisableWeapon = false });
            dstManager.AddComponentData (entity, new BurstSkill {
                skillShootRecoveryTime = skillShootRecoveryTime,
                    skillShootSpeed = skillShootSpeed,
                    offsetX = offsetX,
                    bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab)
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}