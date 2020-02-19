using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight {
    [Serializable]
    struct SelfTag : IComponentData { }

    class FollowCamera : IComponentData {
        public Transform transform;
    }

    [RequiresEntityConversion]
    class PlayerControllerBehaviour : MonoBehaviour, IConvertGameObjectToEntity {
        public Transform followCameraTransform = null;
        public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData (entity, new SelfTag ());
            dstManager.AddComponentObject (entity, new FollowCamera { transform = followCameraTransform });
        }
    }

    class PlayerControllerSystem : ComponentSystem {
        protected override void OnUpdate () {
            float2 moveDir = float2.zero;
            if (Input.GetKey (KeyCode.A))
                moveDir += new float2 (-1, 0);
            if (Input.GetKey (KeyCode.D))
                moveDir += new float2 (1, 0);
            if (Input.GetKey (KeyCode.W))
                moveDir += new float2 (0, 1);
            if (Input.GetKey (KeyCode.S))
                moveDir += new float2 (0, -1);
            moveDir = math.normalizesafe (moveDir);
            bool isJump = Input.GetKey (KeyCode.Space);
            float2 shootDir = math.normalizesafe (new float2 (Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2));
            bool isShoot = Input.GetMouseButton (0);
            bool isCastSkill = Input.GetKey (KeyCode.F);
            Entities.WithAllReadOnly (typeof (SelfTag), typeof (Translation)).ForEach ((FollowCamera followCameraCmpt, ref Translation translationCmpt, ref MoveInput moveInputCmpt, ref JumpInput jumpInputCmpt, ref ShootInput shootInputCmpt, ref SkillInput skillInputCmpt) => {
                followCameraCmpt.transform.position = translationCmpt.Value;
                moveInputCmpt.dir = moveDir;
                jumpInputCmpt.isJump = isJump;
                shootInputCmpt.dir = shootDir;
                shootInputCmpt.isShoot = isShoot;
                skillInputCmpt.isCast = isCastSkill;
            });
        }
    }
}