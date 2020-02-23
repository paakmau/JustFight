using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace JustFight.Tank {

    [RequiresEntityConversion]
    class FollowTankHullAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankHullToFollow> (entity);
            dstManager.RemoveComponent<Translation> (entity);
        }
    }
}