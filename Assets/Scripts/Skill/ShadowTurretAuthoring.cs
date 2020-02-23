using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class ShadowTurretAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<ShadowTurret> (entity);
        }
    }
}