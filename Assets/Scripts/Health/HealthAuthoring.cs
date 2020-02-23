using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Health {

    [RequiresEntityConversion]
    class HealthAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject healthBarPrefab = null;
        public int maxHealth = 100;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new HealthPoint { maxValue = maxHealth, value = maxHealth });
            dstManager.AddComponentData (entity, new HealthBarPrefab { entity = conversionSystem.GetPrimaryEntity (healthBarPrefab) });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (healthBarPrefab);
        }
    }
}