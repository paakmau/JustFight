using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class TankHullAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public float moveSpeed = 7;
        public float jumpSpeed = 10;
        public float jumpRecoveryTime = 2f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankHullTeam> (entity);
            dstManager.AddComponent<MoveInput> (entity);
            dstManager.AddComponent<JumpInput> (entity);
            dstManager.AddComponentData (entity, new MoveSpeed { value = moveSpeed });
            dstManager.AddComponentData (entity, new JumpState { speed = jumpSpeed, recoveryTime = jumpRecoveryTime });
            dstManager.AddComponent<TankTurretInstance> (entity);
        }
    }
}