using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct LifeRegenerationFountainSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<LifeRegenerationZone>();
        state.RequireForUpdate<HealthComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;


        foreach (var (healthComponent, localTransform, physicsVelocity,entity) in SystemAPI.Query<RefRW<HealthComponent>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            if (healthComponent.ValueRW.currentHealth >= healthComponent.ValueRO.maxHealth) continue;

            NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);

            bool Collision = CollisionWorld.SphereCastAll(localTransform.ValueRO.Position, 1f, math.normalize(localTransform.ValueRO.Position), 1, ref hits, new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = (uint)1 << 3,
                GroupIndex = 0
            });
            if (hits.Length > 0)
            {
                // UnityEngine.Debug.Log($"Health: {healthComponent.ValueRW.currentHealth}");
                // UnityEngine.Debug.Log($"hits: {hits.Length}");

                healthComponent.ValueRW.currentHealth += 5 * SystemAPI.Time.DeltaTime;
                physicsVelocity.ValueRW.Linear -= 5f * SystemAPI.Time.DeltaTime;
                state.EntityManager.SetComponentEnabled<UpdateHealthBarUI>(entity, true);

            }

            hits.Dispose();
        }

    }
}
