using Unity.Entities;
using UnityEngine;

namespace JustFight {

    [RequiresEntityConversion]
    class EnemyActionAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new EnemyAction { random = new Unity.Mathematics.Random ((uint) System.DateTime.Now.Millisecond % 1000) });
        }
    }
}