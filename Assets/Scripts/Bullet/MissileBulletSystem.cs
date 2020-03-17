using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace JustFight.Bullet {

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
    class MissileBulletSystem : SystemBase {

        BlobAssetReference<Unity.Physics.Collider> m_sphereCollider;
        BuildPhysicsWorld m_buildPhysicsWorldSystem;
        EndFramePhysicsSystem m_endFramePhysicsSystem;
        protected override void OnCreate () {
            m_sphereCollider = SphereCollider.Create (
                new SphereGeometry { Center = float3.zero, Radius = 4 },
                new CollisionFilter { BelongsTo = ~0u, CollidesWith = 1u, GroupIndex = 0 }
            );
            m_buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            m_endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem> ();
        }
        protected override unsafe void OnUpdate () {
            var collisionWorld = m_buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;
            var sphereCollider = m_sphereCollider;
            Dependency = JobHandle.CombineDependencies (m_endFramePhysicsSystem.FinalJobHandle, Dependency);
            Dependency = Entities.WithReadOnly (collisionWorld).ForEach ((ref PhysicsVelocity velocityCmpt, ref Rotation rotationCmpt, in MissileBullet missileBulletTargetCmpt, in LocalToWorld localToWorldCmpt) => {
                var pos = localToWorldCmpt.Position;
                var forward = localToWorldCmpt.Forward;
                var vL = math.length (velocityCmpt.Linear);
                var vDir = velocityCmpt.Linear / vL;
                ColliderCastHit closestHit;
                bool hasTarget = collisionWorld.CastCollider (new ColliderCastInput {
                    Collider = (Unity.Physics.Collider * ) sphereCollider.GetUnsafePtr (),
                        Start = pos + vDir * 5f,
                        End = pos + vDir * 20
                }, out closestHit);
                if (hasTarget) {
                    var targetDir = math.normalize (closestHit.Position - pos);
                    var dir = targetDir - vDir;
                    dir.y = 0;
                    velocityCmpt.Linear = math.normalize (velocityCmpt.Linear + dir) * vL;
                    rotationCmpt.Value = quaternion.LookRotation (vDir, math.up ());
                }
            }).ScheduleParallel (Dependency);
        }
    }
}