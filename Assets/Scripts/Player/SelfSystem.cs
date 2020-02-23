using JustFight.TankHull;
using JustFight.TankTurret;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace JustFight.Player {

    class SelfSystem : ComponentSystem {
        protected override void OnUpdate () {
            float3 moveDir = float3.zero;
            if (Input.GetKey (KeyCode.A))
                moveDir += new float3 (-1, 0, 0);
            if (Input.GetKey (KeyCode.D))
                moveDir += new float3 (1, 0, 0);
            if (Input.GetKey (KeyCode.W))
                moveDir += new float3 (0, 0, 1);
            if (Input.GetKey (KeyCode.S))
                moveDir += new float3 (0, 0, -1);
            moveDir = math.normalizesafe (moveDir);
            bool isJump = Input.GetKey (KeyCode.Space);
            float3 shootDir = math.normalizesafe (new float3 (Input.mousePosition.x - Screen.width / 2, 0, Input.mousePosition.y - Screen.height / 2));
            bool isShoot = Input.GetMouseButton (0);
            bool isCastSkill = Input.GetKey (KeyCode.F);
            Entities.WithAllReadOnly (typeof (SelfHull), typeof (Translation)).ForEach ((FollowCamera followCameraCmpt, ref Translation translationCmpt, ref MoveInput moveInputCmpt, ref JumpInput jumpInputCmpt) => {
                followCameraCmpt.transform.position = translationCmpt.Value;
                moveInputCmpt.dir = moveDir;
                jumpInputCmpt.isJump = isJump;
            });
            Entities.WithAllReadOnly (typeof (SelfTurret)).ForEach ((ref ShootInput shootInputCmpt, ref SkillInput skillInputCmpt) => {
                shootInputCmpt.dir = shootDir;
                shootInputCmpt.isShoot = isShoot;
                skillInputCmpt.isCast = isCastSkill;
            });
        }
    }
}