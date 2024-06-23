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
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        // var PlayerTargetPosition = SystemAPI.GetSingleton<PlayerTargetPosition>();

        // if (PlayerTargetPosition.targetClickPosition.Equals(priorMousePositionAfterButtonPress.targetClickPosition) )
        // {
        //     return;
        // }
        // priorMousePositionAfterButtonPress.targetClickPosition = PlayerTargetPosition.targetClickPosition;
        var CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
      
        foreach ((RefRW<LocalTransform> localTransform, RefRW<PlayerMovementComponent> player, RefRW<PlayerTargetPosition> targetPosition,var velocity) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerMovementComponent>, RefRW<PlayerTargetPosition>,RefRW<PhysicsVelocity>>())
        {     
            float3 currentPosition = localTransform.ValueRW.Position; // Assuming LocalTransform has a Value with a Position
            float3 direction = math.normalize(targetPosition.ValueRO.targetClickPosition - currentPosition);
           
            if (!player.ValueRO.isGrounded)
            {
                break; // Skip this entity if it's not grounded
            }
            // Perform raycast to find the terrain height at the target position
            RaycastInput raycastInput = new RaycastInput()
            {
                Start = currentPosition,
                End = currentPosition + new float3(0, -5f, 0),
                Filter = new CollisionFilter()
                {
                    BelongsTo = (uint)0,
                    CollidesWith = (uint)4,
                    GroupIndex = 0
                }
            };
            
            if (CollisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
            {
                // Adjust the position based on the terrain height
                currentPosition.y = hit.Position.y;
                //Debug.Log("hit.Position.y: "+hit.Position.y);
                localTransform.ValueRW.Position = currentPosition;
            }
            // Update the position only if the target is not reached
            // Check for all components including Z to ensure we're working in 3D
            if (!math.all(math.abs(currentPosition - targetPosition.ValueRO.targetClickPosition) <= new float3(1.5f, 1.5f, 1.5f)))
            {
                
                velocity.ValueRW.Linear = direction * player.ValueRO.speed ;
                float3 flatDirection = new float3(direction.x, 0, direction.z);
                quaternion targetRotation = quaternion.LookRotationSafe(flatDirection, math.up());
                localTransform.ValueRW.Rotation = targetRotation;
                player.ValueRW.isWalking= true; 

            }else{
                player.ValueRW.isWalking= false; 
                velocity.ValueRW.Linear = float3.zero;
                velocity.ValueRW.Angular = float3.zero;
            }
            
        }
    }
}
