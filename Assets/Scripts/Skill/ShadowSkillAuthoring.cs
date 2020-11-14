using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace JustFight.Skill {

    class ShadowSkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
        public float recoveryTime = 8;
        public GameObject shadowHullPrefab = null;
        public GameObject shadowTurretPrefab = null;
        public float skillLastTime = 2.5f;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new Skill { recoveryLeftTime = recoveryTime, recoveryTime = recoveryTime, lastTime = skillLastTime });
            dstManager.AddComponentData (entity, new ShadowSkill {
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