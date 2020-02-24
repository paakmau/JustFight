using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace JustFight.Tank {

    [RequiresEntityConversion]
    class TankTurretAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankTurretTeam> (entity);
            dstManager.AddComponentData (entity, new AimInput { dir = new float3 (1, 0, 0) });
        }
    }
}