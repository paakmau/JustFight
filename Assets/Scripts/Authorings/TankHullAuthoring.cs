using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class TankHullAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float jumpSpeed = 10;
        public float jumpRecoveryTime = 1.25f;
        public GameObject healthBarPrefab = null;
        public int maxHealth = 100;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<TankHullTeam> (entity);
            dstManager.AddComponent<MoveInput> (entity);
            dstManager.AddComponent<JumpInput> (entity);
            dstManager.AddComponentData (entity, new JumpState { speed = jumpSpeed, recoveryTime = jumpRecoveryTime });
            dstManager.AddComponentData (entity, new Health { maxValue = maxHealth, value = maxHealth });
            dstManager.AddComponentData (entity, new HealthBarPrefab { entity = conversionSystem.GetPrimaryEntity (healthBarPrefab) });
            dstManager.AddComponent<TankTurretInstance> (entity);
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (healthBarPrefab);
        }
    }
}