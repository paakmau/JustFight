using JustFight.FollowCamera;
using JustFight.Tank;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight.Input {

    class SelfSystem : SystemBase {
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
            bool isCastSkill = UnityEngine.Input.GetKey (KeyCode.F);
            FollowCameraTransform followCameraTransform = ComponentSystemBaseManagedComponentExtensions.GetSingleton<FollowCameraTransform> (this);
            Entities.ForEach ((ref MoveInput moveInput, ref JumpInput jumpInput, in SelfHull selfHull, in Translation translation) => {
                followCameraTransform.transform.position = translation.Value;
                moveInput.dir = moveDir;
                jumpInput.isJump = isJump;
            });
            Entities.ForEach ((ref AimInput aimInput, in SelfTurret selfTurret) => {
                aimInput.dir = shootDir;
                aimInput.isCast = isCastSkill;
            });
        }
    }
}