using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
// [UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))] // Ensure it runs after command buffers are played back
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct DestructionSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DestroyTag>();
        
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // //EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<DestroyTag>());
        // var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        // foreach((var DestroyTag ,var e)in SystemAPI.Query<EnabledRefRO<DestroyTag>>().WithEntityAccess())
        // {
        //     if (DestroyTag.ValueRO == true)
        //     {ecb.DestroyEntity(e);}
        // }

        // //state.EntityManager.DestroyEntity(query);
        // // You are responsible for disposing of any ECB you create.


        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_, entity) in SystemAPI.Query<DestroyTag>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);

            if (state.EntityManager.HasBuffer<Child>(entity))
            {
                var children = SystemAPI.GetBuffer<Child>(entity);
                foreach (var child in children)
                {
                    ecb.AddComponent<DestroyTag>(child.Value);
                }
            }
        }
       

        foreach (var (enemyHealthComponent, entity) in SystemAPI.Query<RefRO<HealthComponent>>().WithDisabled<DestroyTag>().WithEntityAccess())
        {
            if (enemyHealthComponent.ValueRO.currentHealth <= 0)
            {
                state.EntityManager.SetComponentEnabled<DestroyTag>(entity, true);
                var scrore = SystemAPI.GetSingletonRW<ScoreComponent>();
                scrore.ValueRW.Value += 1;
            }
        }
        foreach (var (health, destroyTag, entity) in SystemAPI.Query<RefRO<HealthComponent>, RefRO<GameOverOnDestroy>>().WithEntityAccess())
        {
            if (health.ValueRO.currentHealth <= 0)
            {
                //state.EntityManager.SetComponentEnabled<DestroyTag>(entity, true);
                //var gameOverEntity = ecb.CreateEntity();
                ecb.AddComponent<GameOverTag>(entity);

            }


        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}