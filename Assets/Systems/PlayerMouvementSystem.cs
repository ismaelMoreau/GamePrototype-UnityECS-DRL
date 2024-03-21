using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerMouvementSystem : ISystem
{
    // PlayerTargetPosition priorMousePositionAfterButtonPress;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTargetPosition>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        // var PlayerTargetPosition = SystemAPI.GetSingleton<PlayerTargetPosition>();

        // if (PlayerTargetPosition.targetClickPosition.Equals(priorMousePositionAfterButtonPress.targetClickPosition) )
        // {
        //     return;
        // }
        // priorMousePositionAfterButtonPress.targetClickPosition = PlayerTargetPosition.targetClickPosition;
        
      
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed, RefRW<PlayerTargetPosition> targetPosition) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<PlayerTargetPosition>>())
        {     
            float3 currentPosition = localTransform.ValueRW.Position; // Assuming LocalTransform has a Value with a Position
            float3 direction = math.normalize(targetPosition.ValueRO.targetClickPosition - currentPosition);

            // Update the position only if the target is not reached
            // Check for all components including Z to ensure we're working in 3D
            if (!math.all(math.abs(currentPosition - targetPosition.ValueRO.targetClickPosition) < new float3(0.05f, 0.05f, 0.05f)))
            {
                localTransform.ValueRW.Position += direction * playerSpeed.ValueRO.speed * SystemAPI.Time.DeltaTime;
            }
        }
    }
}
