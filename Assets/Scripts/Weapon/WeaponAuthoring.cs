using Unity.Entities;
using UnityEngine;

namespace JustFight.Weapon {

    class WeaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public float shootRecoveryTime = 0.4f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new WeaponState { recoveryLeftTime = shootRecoveryTime, recoveryTime = shootRecoveryTime });
        }
    }
}