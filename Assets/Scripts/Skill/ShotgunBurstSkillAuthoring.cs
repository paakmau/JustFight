using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class ShotgunBurstSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public float skillShootSpeed = 16;
        public int bulletNumPerShoot = 8;
        public GameObject bulletPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new ShotgunBurstSkill {
                skillLastTime = skillLastTime,
                    skillShootRecoveryTime = skillShootRecoveryTime,
                    skillShootSpeed = skillShootSpeed,
                    bulletNumPerShoot = bulletNumPerShoot,
                    bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab)
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}