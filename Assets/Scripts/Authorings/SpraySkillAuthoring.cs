using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class SpraySkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public float skillRecoveryTime = 5;
        public float skillLastTime = 1.5f;
        public float skillShootRecoveryTime = 0.15f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new SpraySkill { skillRecoveryTime = skillRecoveryTime, skillShootRecoveryTime = skillShootRecoveryTime, skillLastTime = skillLastTime });
        }
    }
}