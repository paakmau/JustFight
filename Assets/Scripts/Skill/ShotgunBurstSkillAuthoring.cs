using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class ShotgunBurstSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 8;
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public float skillShootSpeed = 16;
        public int bulletNumPerShoot = 8;
        public float spread = 0.5f;
        public GameObject bulletPrefab = null;
        public float3 offset = default;
        public float upRot = 0;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Skill { recoveryLeftTime = recoveryTime, recoveryTime = recoveryTime, lastTime = skillLastTime, isDisableWeapon = true });
            dstManager.AddComponentData (entity, new ShotgunBurstSkill {
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