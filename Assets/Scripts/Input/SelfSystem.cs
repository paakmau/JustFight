using JustFight.Tank;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight.Input {

    class SelfSystem : ComponentSystem {
        protected override void OnUpdate () {
            float3 moveDir = float3.zero;
            if (UnityEngine.Input.GetKey (KeyCode.A))
                moveDir += new float3 (-1, 0, 0);
            if (UnityEngine.Input.GetKey (KeyCode.D))
                moveDir += new float3 (1, 0, 0);
            if (UnityEngine.Input.GetKey (KeyCode.W))
                moveDir += new float3 (0, 0, 1);
            if (UnityEngine.Input.GetKey (KeyCode.S))
                moveDir += new float3 (0, 0, -1);
            moveDir = math.normalizesafe (moveDir);
            bool isJump = UnityEngine.Input.GetKey (KeyCode.Space);
            float3 shootDir = math.normalizesafe (new float3 (UnityEngine.Input.mousePosition.x - Screen.width / 2, 0, UnityEngine.Input.mousePosition.y - Screen.height / 2));
            bool isShoot = UnityEngine.Input.GetMouseButton (0);
            bool isCastSkill = UnityEngine.Input.GetKey (KeyCode.F);
            Entities.WithAllReadOnly (typeof (SelfHull), typeof (Translation)).ForEach ((FollowCamera followCameraCmpt, ref Translation translationCmpt, ref MoveInput moveInputCmpt, ref JumpInput jumpInputCmpt) => {
                followCameraCmpt.transform.position = translationCmpt.Value;
                moveInputCmpt.dir = moveDir;
                jumpInputCmpt.isJump = isJump;
            });
            Entities.WithAllReadOnly (typeof (SelfTurret)).ForEach ((ref AimInput aimInputCmpt) => {
                aimInputCmpt.dir = shootDir;
                aimInputCmpt.isShoot = isShoot;
                aimInputCmpt.isCast = isCastSkill;
            });
        }
    }
}