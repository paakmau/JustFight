using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class ShotgunBurstSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public float skillShootSpeed = 16;
        public int bulletNumPerShoot = 8;
        public float spread = 0.5f;
        public GameObject bulletPrefab = null;
        public float3 offset = default;
        public float upRot = 0;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new ShotgunBurstSkill {
                skillLastTime = skillLastTime,
                    skillShootRecoveryTime = skillShootRecoveryTime,
                    skillShootSpeed = skillShootSpeed,
                    bulletNumPerShoot = bulletNumPerShoot,
                    spread = spread,
                    bulletPrefab = conversionSystem.GetPrimaryEntity (bulletPrefab),
                    offset = offset,
                    upRot = upRot
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (bulletPrefab);
        }
    }
}