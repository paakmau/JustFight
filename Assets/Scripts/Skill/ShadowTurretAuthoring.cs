using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    class ShadowTurretAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<ShadowTurret> (entity);
        }
    }
}