using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class SelfSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject hullPrefab = null;
        public GameObject turretPrefab = null;
        public int teamId = 1;
        public Transform followCameraTransform = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var selfSpawnerCmpt = new SelfSpawner {
                hullPrefab = conversionSystem.GetPrimaryEntity (hullPrefab),
                turretPrefab = conversionSystem.GetPrimaryEntity (turretPrefab),
                teamId = teamId,
                followCameraTransform = followCameraTransform
            };
            dstManager.AddComponentData (entity, selfSpawnerCmpt);
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (hullPrefab);
            referencedPrefabs.Add (turretPrefab);
        }
    }
}