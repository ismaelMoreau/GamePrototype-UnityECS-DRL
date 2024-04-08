using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
// [UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))] // Ensure it runs after command buffers are played back
public partial struct DestructionSystem : ISystem
{
    
    [BurstCompile]    
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<DestroyTag>();
        
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<DestroyTag>());
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        foreach((var DestroyTag ,var e)in SystemAPI.Query<EnabledRefRO<DestroyTag>>().WithEntityAccess())
        {
            if (DestroyTag.ValueRO == true)
            {ecb.DestroyEntity(e);}
        }
        
        //state.EntityManager.DestroyEntity(query);
        // You are responsible for disposing of any ECB you create.
       
    }
}
