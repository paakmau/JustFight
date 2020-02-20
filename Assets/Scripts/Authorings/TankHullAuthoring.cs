using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class TankHullAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public int teamId = 0;
        public float jumpSpeed = 10;
        public float jumpRecoveryTime = 1.25f;
        public GameObject healthBarPrefab = null;
        public int maxHealth = 100;
        public GameObject tankTurretPrefab = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new TankHullTeam { id = teamId });
            dstManager.AddComponent<MoveInput> (entity);
            dstManager.AddComponent<JumpInput> (entity);
            dstManager.AddComponentData (entity, new JumpState { speed = jumpSpeed, recoveryTime = jumpRecoveryTime });
            dstManager.AddComponentData (entity, new Health { maxValue = maxHealth, value = maxHealth });
            dstManager.AddComponentData (entity, new HealthBarPrefab { entity = conversionSystem.GetPrimaryEntity (healthBarPrefab) });
            dstManager.AddComponentData (entity, new TankTurretPrefab { entity = conversionSystem.GetPrimaryEntity (tankTurretPrefab) });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (healthBarPrefab);
            referencedPrefabs.Add (tankTurretPrefab);
        }
    }
}