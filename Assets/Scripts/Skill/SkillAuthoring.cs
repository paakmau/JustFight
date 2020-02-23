using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class SkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public float recoveryTime = 6;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Skill { leftTime = recoveryTime, recoveryTime = recoveryTime });
        }
    }
}