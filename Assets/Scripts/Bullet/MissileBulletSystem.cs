using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace JustFight.Bullet {

    [UpdateAfter (typeof (EndFramePhysicsSystem))]
    class MissileBulletSystem : SystemBase {

        [BurstCompile]
        struct MissileJob : IJobForEach<MissileBullet, PhysicsVelocity, Rotation, LocalToWorld> {
            [ReadOnly] public float dT;
            [ReadOnly] public CollisionWorld collisionWorld;
            [ReadOnly] public BlobAssetReference<Unity.Physics.Collider> sphereCollider;
            public unsafe void Execute ([ReadOnly] ref MissileBullet missileBulletTargetCmpt, ref PhysicsVelocity velocityCmpt, ref Rotation rotationCmpt, [ReadOnly] ref LocalToWorld localToWorldCmpt) {
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
                    velocityCmpt.Linear = math.normalize (velocityCmpt.Linear + dir) * vL;
                    rotationCmpt.Value = quaternion.LookRotation (vDir, math.up ());
                }
            }
        }

        BlobAssetReference<Unity.Physics.Collider> sphereCollider;
        BuildPhysicsWorld buildPhysicsWorldSystem;
        EndFramePhysicsSystem endFramePhysicsSystem;
        protected override void OnCreate () {
            sphereCollider = Unity.Physics.SphereCollider.Create (
                new SphereGeometry { Center = float3.zero, Radius = 4 },
                new CollisionFilter { BelongsTo = ~0u, CollidesWith = 1u, GroupIndex = 0 }
            );
            buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld> ();
            endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem> ();
        }
        protected override void OnUpdate () {
            Dependency = JobHandle.CombineDependencies(endFramePhysicsSystem.FinalJobHandle, Dependency);
            Dependency = new MissileJob {
                dT = Time.DeltaTime, collisionWorld = buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld, sphereCollider = sphereCollider
            }.Schedule (this, Dependency);
        }
    }
}