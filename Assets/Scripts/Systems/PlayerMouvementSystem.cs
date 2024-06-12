using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
    
    //[BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        // var PlayerTargetPosition = SystemAPI.GetSingleton<PlayerTargetPosition>();

        // if (PlayerTargetPosition.targetClickPosition.Equals(priorMousePositionAfterButtonPress.targetClickPosition) )
        // {
        //     return;
        // }
        // priorMousePositionAfterButtonPress.targetClickPosition = PlayerTargetPosition.targetClickPosition;
        
      
        foreach ((RefRW<LocalTransform> localTransform, RefRW<PlayerMovementComponent> player, RefRW<PlayerTargetPosition> targetPosition,var velocity) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerMovementComponent>, RefRW<PlayerTargetPosition>,RefRW<PhysicsVelocity>>())
        {     
            float3 currentPosition = localTransform.ValueRW.Position; // Assuming LocalTransform has a Value with a Position
            float3 direction = math.normalize(targetPosition.ValueRO.targetClickPosition - currentPosition);
           
            if (!player.ValueRO.isGrounded)
            {
                break; // Skip this entity if it's not grounded
            }

            // Update the position only if the target is not reached
            // Check for all components including Z to ensure we're working in 3D
            if (!math.all(math.abs(currentPosition - targetPosition.ValueRO.targetClickPosition) <= new float3(1.5f, 1.5f, 1.5f)))
            {
                
                localTransform.ValueRW = localTransform.ValueRO.Translate(direction * player.ValueRO.speed * SystemAPI.Time.DeltaTime);
                localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(direction,math.up());
                player.ValueRW.isWalking= true; 

            }else{
                player.ValueRW.isWalking= false; 
                velocity.ValueRW.Linear = float3.zero;
                velocity.ValueRW.Angular = float3.zero;
            }
            
        }
    }
}
