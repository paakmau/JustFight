using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class TankTurretAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankTurretTeam> (entity);
            dstManager.AddComponent<ShootInput> (entity);
            dstManager.AddComponent<SkillInput> (entity);
        }
    }
}