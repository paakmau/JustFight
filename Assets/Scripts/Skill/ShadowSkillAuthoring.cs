using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    [RequiresEntityConversion]
    class ShadowSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public GameObject shadowHullPrefab = null;
        public GameObject shadowTurretPrefab = null;
        public float skillLastTime = 2.5f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new ShadowSkill {
                skillLastTime = skillLastTime,
                    shadowHullPrefab = conversionSystem.GetPrimaryEntity (shadowHullPrefab),
                    shadowTurretPrefab = conversionSystem.GetPrimaryEntity (shadowTurretPrefab)
            });
        }
        public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add (shadowHullPrefab);
            referencedPrefabs.Add (shadowTurretPrefab);
        }
    }
}