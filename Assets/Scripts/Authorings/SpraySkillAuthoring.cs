using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class SpraySkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public float skillRecoveryTime = 6;
        public float skillLastTime = 2.5f;
        public float skillShootRecoveryTime = 0.15f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new SpraySkill { skillRecoveryLeftTime = skillRecoveryTime, skillRecoveryTime = skillRecoveryTime, skillShootRecoveryTime = skillShootRecoveryTime, skillLastTime = skillLastTime });
        }
    }
}